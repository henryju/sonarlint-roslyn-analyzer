﻿using System;
namespace Tests.Diagnostics
{
    class MyAttribute : Attribute
    {

    }
    class AttributeOne : Attribute // Noncompliant
    {

    }
    class ExceptionOne : Exception // Noncompliant
    {

    }
    class MyEventArgsOne : EventArgs // Noncompliant
    {

    }
    class MyEventArgs
    {

    }

    class ExceptionTwo : ExceptionOne // Compliant, the base class doesn't correspond to the naming convention
    {

    }
}
