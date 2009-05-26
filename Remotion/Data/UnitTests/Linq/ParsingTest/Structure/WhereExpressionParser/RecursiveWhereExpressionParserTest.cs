// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.WhereExpressionParserTest;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.WhereExpressionParserTest
{
  [TestFixture]
  public class RecursiveWhereExpressionParserTest
  {
    private IQueryable<Student> _querySource;
    private MethodCallExpression _expression;
    private ExpressionTreeNavigator _navigator;
    private BodyHelper _bodyWhereHelper;
    private ParseResultCollector _result;

    [SetUp]
    public void SetUp ()
    {
      _querySource = ExpressionHelper.CreateQuerySource ();
      _expression = WhereTestQueryGenerator.CreateMultiWhereQuery_WhereExpression (_querySource);
      _navigator = new ExpressionTreeNavigator(_expression);
      _result = new ParseResultCollector (_expression);
      new WhereExpressionParser (true).Parse(_result, _expression);
      _bodyWhereHelper = new BodyHelper (_result.BodyExpressions);
    }
    
    [Test]
    public void ParsesFromExpressions ()
    {
      Assert.IsNotNull (_bodyWhereHelper.FromExpressions);
      Assert.That (_bodyWhereHelper.FromExpressions, Is.EqualTo (new object[] { _navigator.Arguments[0].Arguments[0].Arguments[0].Expression }));
      Assert.IsInstanceOfType (typeof (ConstantExpression), _bodyWhereHelper.FromExpressions[0]);
      Assert.AreSame (_querySource, ((ConstantExpression) _bodyWhereHelper.FromExpressions[0]).Value);
    }

    [Test]
    public void ParsesFromIdentifiers ()
    {
      Assert.IsNotNull (_bodyWhereHelper.FromIdentifiers);
      Assert.That (_bodyWhereHelper.FromIdentifiers,
          Is.EqualTo (new object[] { _navigator.Arguments[0].Arguments[0].Arguments[1].Operand.Parameters[0].Expression }));
      Assert.IsInstanceOfType (typeof (ParameterExpression), _bodyWhereHelper.FromIdentifiers[0]);
      Assert.AreEqual ("s", ((ParameterExpression) _bodyWhereHelper.FromIdentifiers[0]).Name);
    }

    [Test]
    public void ParsesBoolExpressions ()
    {
      Assert.IsNotNull (_bodyWhereHelper.WhereExpressions);
      Assert.That (_bodyWhereHelper.WhereExpressions, Is.EqualTo (new object[] 
          {_navigator.Arguments[0].Arguments[0].Arguments[1].Operand.Expression,
              _navigator.Arguments[0].Arguments[1].Operand.Expression,
              _navigator.Arguments[1].Operand.Expression
          }));
      Assert.IsInstanceOfType (typeof (LambdaExpression), _bodyWhereHelper.WhereExpressions[0]);
      Assert.IsInstanceOfType (typeof (LambdaExpression), _bodyWhereHelper.WhereExpressions[1]);
      Assert.IsInstanceOfType (typeof (LambdaExpression), _bodyWhereHelper.WhereExpressions[2]);
    }

    [Test]
    public void ParsesProjectionExpressions ()
    {
      Assert.IsNotNull (_result.ProjectionExpressions);
      Assert.AreEqual (1, _result.ProjectionExpressions.Count);
      Assert.That (_result.ProjectionExpressions[0].Parameters.Count, Is.EqualTo (1));
      Assert.That (_result.ProjectionExpressions[0].Parameters[0].Name, Is.EqualTo ("s"));
      Assert.That (_result.ProjectionExpressions[0].Parameters[0].Type, Is.EqualTo (typeof (Student)));
      Assert.That (_result.ProjectionExpressions[0].Body, Is.SameAs (_result.ProjectionExpressions[0].Parameters[0]));
    }

    [Test]
    public void ParsesProjectionExpressions_NotTopLevel ()
    {
      _result = new ParseResultCollector (_expression);
      new WhereExpressionParser (false).Parse (_result, _expression);
      Assert.IsNotNull (_result.ProjectionExpressions);
      Assert.AreEqual (0, _result.ProjectionExpressions.Count);
    }
  }
}