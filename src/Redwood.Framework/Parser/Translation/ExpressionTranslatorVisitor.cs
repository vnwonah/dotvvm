﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Redwood.Framework.Resources;

namespace Redwood.Framework.Parser.Translation
{
    public class ExpressionTranslatorVisitor : CSharpSyntaxVisitor<string>
    {

        /// <summary>
        /// Gets a value indicating whether the syntax contains an expression that prevents us to pass the knockout observable as a result.
        /// </summary>
        public bool IsExpression { get; private set; }

        /// <summary>
        /// Visits the prefix unary expression.
        /// </summary>
        public override string VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            IsExpression = true;

            if (node.OperatorToken.IsKind(SyntaxKind.MinusToken))
            {
                return "-" + Visit(node.Operand);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.ExclamationToken))
            {
                return "!" + Visit(node.Operand);
            }

            throw new ParserException(string.Format(Parser_RwHtml.Binding_UnsupportedOperator, node.OperatorToken.Text));
        }

        /// <summary>
        /// Visits the binary expression.
        /// </summary>
        public override string VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            IsExpression = true;

            // arithmetic
            if (node.OperatorToken.IsKind(SyntaxKind.PlusToken))
            {
                return Visit(node.Left) + " + " + Visit(node.Right);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.MinusToken))
            {
                return Visit(node.Left) + " - " + Visit(node.Right);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.AsteriskToken))
            {
                return Visit(node.Left) + " * " + Visit(node.Right);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.SlashToken))
            {
                return Visit(node.Left) + " / " + Visit(node.Right);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.PercentToken))
            {
                return Visit(node.Left) + " % " + Visit(node.Right);
            }

            // comparison
            if (node.OperatorToken.IsKind(SyntaxKind.LessThanToken))
            {
                return Visit(node.Left) + " < " + Visit(node.Right);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.LessThanEqualsToken))
            {
                return Visit(node.Left) + " <= " + Visit(node.Right);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.GreaterThanToken))
            {
                return Visit(node.Left) + " > " + Visit(node.Right);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.GreaterThanEqualsToken))
            {
                return Visit(node.Left) + " >= " + Visit(node.Right);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken))
            {
                return Visit(node.Left) + " === " + Visit(node.Right);
            }
            if (node.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken))
            {
                return Visit(node.Left) + " !== " + Visit(node.Right);
            }

            // null coalescing operator
            if (node.OperatorToken.IsKind(SyntaxKind.QuestionQuestionToken))
            {
                return Visit(node.Left) + " != null ? " + Visit(node.Left) + " : " + Visit(node.Right);
            }

            throw new ParserException(string.Format(Parser_RwHtml.Binding_UnsupportedOperator, node.OperatorToken.Text));
        }

        /// <summary>
        /// Visits the conditional expression.
        /// </summary>
        public override string VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            IsExpression = true;

            return Visit(node.Condition) + " ? " + Visit(node.WhenTrue) + " : " + Visit(node.WhenFalse);
        }

        /// <summary>
        /// Visits the name of the identifier.
        /// </summary>
        public override string VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (node.Identifier.Text == "_parent")
            {
                return "$parent";
            }
            else if (node.Identifier.Text == "_root")
            {
                return "$root";
            }
            else if (node.Identifier.Text == "_this")
            {
                return "$data";
            }
            else
            {
                return node.Identifier.Text + "()";
            }
        }

        /// <summary>
        /// Visits the parenthesized expression.
        /// </summary>
        public override string VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            IsExpression = true;

            return "(" + Visit(node.Expression) + ")";
        }

        /// <summary>
        /// Visits the member access expression.
        /// </summary>
        public override string VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            return Visit(node.Expression) + "." + Visit(node.Name);
        }

        /// <summary>
        /// Visits the literal expression.
        /// </summary>
        public override string VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return node.ToString();
            }
            else if (node.IsKind(SyntaxKind.NumericLiteralExpression))
            {
                return Convert.ToDouble(node.Token.Value).ToString(CultureInfo.InvariantCulture);
            }

            return base.VisitLiteralExpression(node);
        }

        public override string DefaultVisit(SyntaxNode node)
        {
            throw new ParserException(string.Format(Parser_RwHtml.Binding_UnsupportedExpression, node));
        }
    }
}