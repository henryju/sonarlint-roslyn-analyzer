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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using CS = SonarLint.Rules.CSharp;
using VB = SonarLint.Rules.VisualBasic;

namespace SonarLint.UnitTest.Rules
{
    [TestClass]
    public class SingleTopLevelTypePerFileTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void SingleTopLevelTypePerFile()
        {
            Verifier.VerifyAnalyzer(@"TestCases\SingleTopLevelTypePerFile.cs", new CS.SingleTopLevelTypePerFile());
            Verifier.VerifyAnalyzer(@"TestCases\SingleTopLevelTypePerFile.vb", new VB.SingleTopLevelTypePerFile());
        }
    }
}
