﻿/*
 * SonarLint for Visual Studio
 * Copyright (C) 2015 SonarSource
 * sonarqube@googlegroups.com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarQube.CSharp.CodeAnalysis.Common;
using SonarQube.CSharp.CodeAnalysis.Common.Sqale;
using SonarQube.CSharp.CodeAnalysis.Helpers;

namespace SonarQube.CSharp.CodeAnalysis.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SqaleConstantRemediation("5min")]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.Understandability)]
    [Rule(DiagnosticId, RuleSeverity, Title, IsActivatedByDefault)]
    [Tags("clumsy", "cwe", "misra")]
    public class RedundantCast : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1905";
        internal const string Title = "Redundant casts should not be used";
        internal const string Description =
            "Unnecessary casting expressions make the code harder to read and understand.";
        internal const string MessageFormat = "Remove this unnecessary cast to \"{0}\".";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Minor;
        internal const bool IsActivatedByDefault = true;

        internal static readonly DiagnosticDescriptor Rule = 
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, 
                RuleSeverity.ToDiagnosticSeverity(), IsActivatedByDefault, 
                helpLinkUri: DiagnosticId.GetHelpLink(),
                description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        private static readonly string[] CastIEnumerableMethods =
        {
            "Cast", "OfType"
        };

    public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var castExpression = (CastExpressionSyntax) c.Node;

                    var expressionType = c.SemanticModel.GetTypeInfo(castExpression.Expression).Type;
                    if (expressionType == null)
                    {
                        return;
                    }

                    var castType = c.SemanticModel.GetTypeInfo(castExpression.Type).Type;
                    if (castType == null)
                    {
                        return;
                    }

                    if (expressionType.Equals(castType))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, castExpression.Type.GetLocation(), 
                            castType.ToDisplayString()));
                    }
                },
                SyntaxKind.CastExpression);
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                    if (methodSymbol == null ||
                        !MethodIsOnIEnumerable(methodSymbol, c.SemanticModel) ||
                        !CastIEnumerableMethods.Contains(methodSymbol.Name))
                    {
                        return;
                    }

                    var elementType = GetElementType(invocation, methodSymbol, c.SemanticModel);
                    if (elementType == null)
                    {
                        return;
                    }

                    var returnType = methodSymbol.ReturnType as INamedTypeSymbol;
                    if (returnType == null ||
                        !returnType.TypeArguments.Any())
                    {
                        return;
                    }

                    var castType = returnType.TypeArguments.First();

                    if (elementType.Equals(castType))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, GetReportLocation(invocation),
                            returnType.ToDisplayString()));
                    }
                },
                SyntaxKind.InvocationExpression);
        }

        private static Location GetReportLocation(InvocationExpressionSyntax invocation)
        {
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            return memberAccess == null 
                ? invocation.Expression.GetLocation() 
                : memberAccess.Name.GetLocation();
        }

        private static ITypeSymbol GetElementType(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol,
            SemanticModel semanticModel)
        {
            ExpressionSyntax collection;
            if (methodSymbol.MethodKind == MethodKind.Ordinary)
            {
                if (!invocation.ArgumentList.Arguments.Any())
                {
                    return null;
                }
                collection = invocation.ArgumentList.Arguments.First().Expression;
            }
            else
            {
                var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                if (memberAccess == null)
                {
                    return null;
                }
                collection = memberAccess.Expression;
            }

            var collectionType = semanticModel.GetTypeInfo(collection).Type as INamedTypeSymbol;
            if (collectionType != null &&
                collectionType.TypeArguments.Count() == 1)
            {
                return collectionType.TypeArguments.First();
            }

            var arrayType = semanticModel.GetTypeInfo(collection).Type as IArrayTypeSymbol;
            return arrayType != null 
                ? arrayType.ElementType 
                : null;
        }

        internal static bool MethodIsOnIEnumerable(IMethodSymbol methodSymbol, SemanticModel semanticModel)
        {
            if (methodSymbol == null)
            {
                return false;
            }

            var enumerableType = semanticModel.Compilation.GetSpecialType(SpecialType.System_Collections_IEnumerable);
            var receiverType = methodSymbol.ReceiverType as INamedTypeSymbol;

            if (methodSymbol.MethodKind == MethodKind.Ordinary)
            {
                if (methodSymbol.Parameters.Count() != 1)
                {
                    return false;
                }
                receiverType = methodSymbol.Parameters.First().Type as INamedTypeSymbol;
            }

            return receiverType != null && receiverType.ConstructedFrom == enumerableType;
        }
    }
}
