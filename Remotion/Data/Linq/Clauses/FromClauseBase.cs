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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Base class for all kinds of from clauses in <see cref="QueryModel"/>
  /// </summary>
  public abstract class FromClauseBase : IClause
  {
    private string _itemName;
    private Type _itemType;
    private Expression _fromExpression;

    /// <summary>
    /// Initializes a new instance of the <see cref="FromClauseBase"/> class.
    /// </summary>
    /// <param name="itemName">A name describing the items generated by the from clause.</param>
    /// <param name="itemType">The type of the items generated by the from clause.</param>
    /// <param name="fromExpression">The <see cref="Expression"/> generating data items for this from clause.</param>
    protected FromClauseBase (string itemName, Type itemType, Expression fromExpression)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("itemName", itemName);
      ArgumentUtility.CheckNotNull ("itemType", itemType);
      ArgumentUtility.CheckNotNull ("fromExpression", fromExpression);

      _itemName = itemName;
      _itemType = itemType;
      _fromExpression = fromExpression;

      JoinClauses = new ObservableCollection<JoinClause> ();
      JoinClauses.ItemInserted += JoinClause_ItemAdded;
      JoinClauses.ItemSet += JoinClause_ItemAdded;
    }

    /// <summary>
    /// Gets or sets a name describing the items generated by this from clause.
    /// </summary>
    public string ItemName
    {
      get { return _itemName; }
      set { _itemName = ArgumentUtility.CheckNotNullOrEmpty ("value",value); }
    }

    /// <summary>
    /// Gets or sets the type of the items generated by this from clause.
    /// </summary>
    public Type ItemType
    {
      get { return _itemType; }
      set { _itemType = ArgumentUtility.CheckNotNull ("value",value); }
    }

    /// <summary>
    /// The expression generating the data items for this from clause.
    /// </summary>
    [DebuggerDisplay ("{Remotion.Data.Linq.StringBuilding.FormattingExpressionTreeVisitor.Format (FromExpression),nq}")]
    public Expression FromExpression
    {
      get { return _fromExpression; }
      set { _fromExpression = ArgumentUtility.CheckNotNull ("value", value); _subQueryFromSource = null; }
    }

    public ObservableCollection<JoinClause> JoinClauses { get; private set; }

    private SubQuery _subQueryFromSource;

    /// <summary>
    /// Method for getting source of a from clause.
    /// </summary>
    /// <param name="databaseInfo"></param>
    /// <returns><see cref="IColumnSource"/></returns>
    public virtual IColumnSource GetColumnSource (IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      // TODO 1250: Hack, remove
      var subQueryExpression = FromExpression as SubQueryExpression;
      if (subQueryExpression != null)
      {
        if (_subQueryFromSource == null)
          _subQueryFromSource = new SubQuery (subQueryExpression.QueryModel, ParseMode.SubQueryInFrom, ItemName);
        return _subQueryFromSource;
      }

      return DatabaseInfoUtility.GetTableForFromClause (databaseInfo, this);
    }

    public void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);
      FromExpression = transformation (FromExpression);

      foreach (var joinClause in JoinClauses)
      {
        joinClause.TransformExpressions (transformation);
      }
    }

    protected void AddClonedJoinClauses (IEnumerable<JoinClause> originalJoinClauses, CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("originalJoinClauses", originalJoinClauses);
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      foreach (var joinClause in originalJoinClauses)
      {
        var joinClauseClone = joinClause.Clone (cloneContext);
        JoinClauses.Add (joinClauseClone);
      }
    }

    private void JoinClause_ItemAdded (object sender, ObservableCollectionChangedEventArgs<JoinClause> e)
    {
      ArgumentUtility.CheckNotNull ("e.Item", e.Item);
    }
  }
}
