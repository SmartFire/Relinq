// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Rhino.Mocks;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;
using Remotion.Utilities;
using System.Collections;

namespace Remotion.Data.UnitTests.Linq
{
  [TestFixture]
  public class QueryProviderBaseTest
  {
    private MockRepository _mockRepository;
    private QueryProviderBase _queryProvider;
    private IQueryExecutor _executor;

    [SetUp]
    public void SetUp()
    {
      _mockRepository = new MockRepository();
      _executor = _mockRepository.StrictMock<IQueryExecutor>();
      _queryProvider = new TestQueryProvider (_executor);
    }

    [Test]
    public void Initialization()
    {
      Assert.IsNotNull (_queryProvider);
    }

    [Test]
    public void CreateQueryReturnsIQueryable()
    {
      Expression expression = ExpressionHelper.CreateExpression();
      IQueryable queryable = _queryProvider.CreateQuery (expression);

      Assert.IsNotNull (queryable);
      Assert.IsInstanceOfType (typeof (IQueryable<int>), queryable);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void CreateQueryUnwrapsException ()
    {
      Expression expression = ExpressionHelper.CreateLambdaExpression ();
      _queryProvider.CreateQuery (expression);
    }

    [Test]
    public void GenericCreateQueryReturnsIQueryable ()
    {
      Expression expression = ExpressionHelper.CreateNewIntArrayExpression ();
      IQueryable<int> queryable = _queryProvider.CreateQuery<int> (expression);

      Assert.IsNotNull (queryable);
    }

    [Test]
    public void GenerateQueryModel ()
    {
      Expression expression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource ());
      var queryModel = _queryProvider.GenerateQueryModel (expression);

      Assert.That (queryModel.GetExpressionTree (), Is.SameAs (expression));
    }

    [Test]
    public void GenericExecute_Single()
    {
      Expression expression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource());
      Expect.Call (_executor.ExecuteSingle (Arg<QueryModel>.Matches (queryModel => queryModel.GetExpressionTree() == expression))).Return (0);

      _mockRepository.ReplayAll();

      _queryProvider.Execute<int> (expression);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void Execute_Single ()
    {
      Expression expression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource ());
      Expect.Call (_executor.ExecuteSingle (Arg<QueryModel>.Matches (queryModel => queryModel.GetExpressionTree () == expression))).Return (0);

      _mockRepository.ReplayAll ();

      _queryProvider.Execute (expression);

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void GenericExecute_Collection ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource(_executor));
      var student = new Student();
      
      Expression expression = query.Expression;
      Expect.Call (_executor.ExecuteCollection  (Arg<QueryModel>.Matches (queryModel => queryModel.GetExpressionTree() == expression)))
          .Return (new[] {student});

      _mockRepository.ReplayAll ();

      var students = new List<Student> (query); // enumerates query -> ExecuteCollection
      Assert.AreEqual (1, students.Count);
      Assert.AreSame (student, students[0]);

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void NonGenericExecute_Collection ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource (_executor));
      var student = new Student ();

      Expression expression = query.Expression;
      Expect.Call (_executor.ExecuteCollection (Arg<QueryModel>.Matches (queryModel => queryModel.GetExpressionTree () == expression)))
          .Return (new[] { student });

      _mockRepository.ReplayAll ();

      var students = new ArrayList();
      IEnumerable nonGenericQuery = query;
      foreach (object o in nonGenericQuery) // enumerates query -> ExecuteCollection
        students.Add (o);

      Assert.AreEqual (1, students.Count);
      Assert.AreSame (student, students[0]);

      _mockRepository.VerifyAll ();
    }
  }
}