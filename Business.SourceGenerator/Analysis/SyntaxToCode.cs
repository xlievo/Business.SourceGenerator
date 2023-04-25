/*==================================
             ########
            ##########
             ########
            ##########
          ##############
         #######  #######
        ######      ######
        #####        #####
        ####          ####
        ####   ####   ####
        #####  ####  #####
         ################
          ##############
==================================*/

namespace Business.SourceGenerator.Analysis
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Linq;

    public readonly struct GlobalConst
    {
        public const string Global = "global::";

        public const string Global_System_Type = "global::System.Type";
        public const string Global_System_Object = "global::System.Object";
        public const string Global_System_String = "global::System.String";
        public const string Global_System_Int32 = "global::System.Int32";
        public const string Global_System_Boolean = "global::System.Boolean";

        public const string System_Type = "System.Type";
        public const string System_Object = "System.Object";
        public const string System_String = "System.String";
        public const string System_Int32 = "System.Int32";
        public const string System_Boolean = "System.Boolean";

        public const string Type = "Type";
        public const string Object = "object";
        public const string String = "string";
        public const string Int32 = "int";
        public const string Boolean = "bool";

        public const string Global_System = "global::System.";
        public const string Global_Business_SourceGenerator = "global::Business.SourceGenerator.";
        public const string Global_Business_SourceGenerator_Meta = "global::Business.SourceGenerator.Meta.";
        public const string Global_System_Collections_Generic = "global::System.Collections.Generic.";
        public const string Global_System_Collections_ObjectModel = "global::System.Collections.ObjectModel.";
        public const string Global_System_Threading_Tasks = "global::System.Threading.Tasks.";
    }

    public static class SyntaxToCode
    {
        #region Global

        public enum GlobalName
        {
            Globa,
            System,
            Business_SourceGenerator_Meta,
            Business_SourceGenerator,
            System_Collections_Generic,
            System_Collections_ObjectModel,
            System_Threading_Tasks,

            System_Type,
            System_Object,
            System_String,
            System_Int32,
            System_Boolean,
        }

        public static string GetGlobalName(this ToCodeOpt opt, GlobalName name)
        {
            switch (name)
            {
                case GlobalName.Globa: return opt.Global ? GlobalConst.Global : default;
                case GlobalName.System: return opt.Global ? GlobalConst.Global_System : default;
                case GlobalName.Business_SourceGenerator_Meta: return opt.Global ? GlobalConst.Global_Business_SourceGenerator_Meta : default;
                case GlobalName.Business_SourceGenerator: return opt.Global ? GlobalConst.Global_Business_SourceGenerator : default;
                case GlobalName.System_Collections_Generic: return opt.Global ? GlobalConst.Global_System_Collections_Generic : default;
                case GlobalName.System_Collections_ObjectModel: return opt.Global ? GlobalConst.Global_System_Collections_ObjectModel : default;
                case GlobalName.System_Threading_Tasks: return opt.Global ? GlobalConst.Global_System_Threading_Tasks : default;

                case GlobalName.System_Type: return opt.Global ? GlobalConst.Global_System_Type : GlobalConst.Type;
                case GlobalName.System_Object: return opt.Global ? GlobalConst.Global_System_Object : GlobalConst.Object;
                case GlobalName.System_String: return opt.Global ? GlobalConst.Global_System_String : GlobalConst.String;
                case GlobalName.System_Int32: return opt.Global ? GlobalConst.Global_System_Int32 : GlobalConst.Int32;
                case GlobalName.System_Boolean: return opt.Global ? GlobalConst.Global_System_Boolean : GlobalConst.Boolean;
                default: return default;
            }
        }

        #endregion
        public readonly struct ToCodeOpt
        {
            public ToCodeOpt(Func<SyntaxNode, string, string> replace = null, bool standardFormat = false, bool global = false)
            {
                Replace = replace;
                StandardFormat = standardFormat;
                Global = global;
            }

            public Func<SyntaxNode, string, string> Replace { get; }

            public bool StandardFormat { get; }

            public bool Global { get; }
        }

        static string GetSign(CSharpSyntaxNode syntaxNode, ToCodeOpt opt)
        {
            if (syntaxNode is null)
            {
                throw new ArgumentNullException(nameof(syntaxNode));
            }

            switch (syntaxNode)
            {
                case MethodDeclarationSyntax node:
                    {
                        var value = $"{ToCode(node.ReturnType, opt)}";

                        if (null != node.ExplicitInterfaceSpecifier)
                        {
                            value = $"{value} {ToCode(node.ExplicitInterfaceSpecifier, opt)}{node.Identifier}";
                        }
                        else
                        {
                            value = $"{value} {node.Identifier}";
                        }

                        if (null != node.TypeParameterList)
                        {
                            value = $"{value}{ToCode(node.TypeParameterList, opt)}";
                        }

                        value = $"{value}{ToCode(node.ParameterList, opt)}";

                        if (node.Modifiers.Any())
                        {
                            value = $"{node.Modifiers} {value}";
                        }

                        if (node.ConstraintClauses.Any())
                        {
                            value = $"{value} {string.Join(" ", node.ConstraintClauses.Select(c => ToCode(c, opt)))}";
                        }

                        return value;
                    }
                case ConstructorDeclarationSyntax node:
                    {
                        var value = $"{node.Identifier}{ToCode(node.ParameterList, opt)}";

                        if (null != node.Initializer)
                        {
                            value = $"{value} {ToCode(node.Initializer, opt)}";
                        }

                        if (node.Modifiers.Any())
                        {
                            value = $"{node.Modifiers} {value}";
                        }

                        return value;
                    }
                case LocalFunctionStatementSyntax node:
                    {
                        var value = $"{ToCode(node.ReturnType, opt)} {node.Identifier}";

                        if (null != node.TypeParameterList)
                        {
                            value = $"{value}{ToCode(node.TypeParameterList, opt)}";
                        }

                        value = $"{value}{ToCode(node.ParameterList, opt)}";

                        if (node.Modifiers.Any())
                        {
                            value = $"{node.Modifiers} {value}";
                        }

                        if (node.ConstraintClauses.Any())
                        {
                            value = $"{value} {string.Join(" ", node.ConstraintClauses.Select(c => ToCode(c, opt)))}";
                        }

                        return value;
                    }
                default: return null;
            }
        }

        static string GetBodyOrExpression(SyntaxNode syntaxNode, ToCodeOpt opt)
        {
            if (syntaxNode is null)
            {
                throw new ArgumentNullException(nameof(syntaxNode));
            }

            switch (syntaxNode)
            {
                case BaseMethodDeclarationSyntax node:
                    return $"{(null != node.ExpressionBody ? ToCode(node.ExpressionBody, opt) : null != node.Body ? ToCode(node.Body, opt) : null)}";

                //var body = (node.ExpressionBody is null && node.ExpressionBody is null) ? default : (node.ExpressionBody is null ? (CSharpSyntaxNode)node.Body : node.ExpressionBody);

                case LocalFunctionStatementSyntax node: return $"{(null != node.ExpressionBody ? ToCode(node.ExpressionBody, opt) : null != node.Body ? ToCode(node.Body, opt) : null)}";
                default: return null;
            }
        }

        /// <summary>
        /// Returns the string representation of this node, not including its leading and trailing trivia.
        /// <para>
        /// replace (node, value) => value
        /// </para>
        /// </summary>
        /// <param name="syntaxNode"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public static string ToCode(this SyntaxNode syntaxNode, ToCodeOpt opt = default)
        {
            if (syntaxNode is null)
            {
                return null;
            }

            var newLine = opt.StandardFormat ? Environment.NewLine : " ";
            var newLine2 = opt.StandardFormat ? $"{newLine}{newLine}" : newLine;
            string value = null;

            switch (syntaxNode)
            {
                #region case
                case UsingDirectiveSyntax node:
                    value = $"{node.UsingKeyword} {node.Name}{node.SemicolonToken}"; break;
                case BaseNamespaceDeclarationSyntax node:
                    string usings = null;

                    if (0 < node.Usings.Count)
                    {
                        usings = $"{string.Join(newLine, node.Usings.Select(c => ToCode(c, opt)))} ";
                    }

                    string members = null;

                    if (0 < node.Members.Count)
                    {
                        members = string.Join(newLine2, node.Members.Select(c => ToCode(c, opt)));
                    }

                    value = $"{usings}{node.NamespaceKeyword} {node.Name}{newLine}{{{newLine}{members}{newLine}}}";
                    break;
                case DefaultExpressionSyntax node:
                    value = $"{node.Keyword}{node.OpenParenToken}{ToCode(node.Type, opt)}{node.CloseParenToken}"; break;
                case ReturnStatementSyntax node:
                    value = node.ReturnKeyword.ToString();
                    if (null != node.Expression)
                    {
                        value = $"{value} {ToCode(node.Expression, opt)}";
                    }
                    value = $"{value}{Semicolon()}";
                    break;
                case BaseListSyntax node:
                    value = $"{node.ColonToken} {string.Join(", ", node.Types.Select(c => ToCode(c, opt)))}";
                    break;
                case ClassDeclarationSyntax node:
                    value = $"{node.Keyword} {node.Identifier}";

                    if (null != node.TypeParameterList)
                    {
                        value = $"{value}{ToCode(node.TypeParameterList, opt)}";
                    }

                    if (null != node.BaseList)
                    {
                        value = $"{value} {ToCode(node.BaseList, opt)}";
                    }

                    if (node.ConstraintClauses.Any())
                    {
                        value = $"{value} {string.Join(" ", node.ConstraintClauses.Select(c => ToCode(c, opt)))}";
                    }

                    if (node.Modifiers.Any())
                    {
                        value = $"{node.Modifiers} {value}";
                    }

                    value = $"{value}{newLine}{node.OpenBraceToken}";

                    if (node.Members.Any())
                    {
                        value = $"{value}{newLine}{string.Join(newLine2, node.Members.Select(c => ToCode(c, opt)))}";
                    }

                    value = $"{value}{newLine}{node.CloseBraceToken}";

                    break;
                case StructDeclarationSyntax node:
                    value = $"{node.Keyword} {node.Identifier}";

                    if (null != node.TypeParameterList)
                    {
                        value = $"{value}{ToCode(node.TypeParameterList, opt)}";
                    }

                    if (null != node.BaseList)
                    {
                        value = $"{value} {ToCode(node.BaseList, opt)}";
                    }

                    if (node.ConstraintClauses.Any())
                    {
                        value = $"{value} {string.Join(" ", node.ConstraintClauses.Select(c => ToCode(c, opt)))}";
                    }

                    if (node.Modifiers.Any())
                    {
                        value = $"{node.Modifiers} {value}";
                    }

                    value = $"{value}{newLine}{node.OpenBraceToken}";

                    if (node.Members.Any())
                    {
                        value = $"{value}{newLine}{string.Join(newLine2, node.Members.Select(c => ToCode(c, opt)))}";
                    }

                    value = $"{value}{newLine}{node.CloseBraceToken}";

                    break;
                case ConstructorInitializerSyntax node:
                    value = $"{node.ColonToken} {node.ThisOrBaseKeyword}{ToCode(node.ArgumentList, opt)}"; break;
                case ConstructorDeclarationSyntax node:
                    value = $"{GetSign(node, opt)}{GetBodyOrExpression(node, opt)}{Semicolon()}"; break;
                case MethodDeclarationSyntax node:
                    value = $"{GetSign(node, opt)}{GetBodyOrExpression(node, opt)}{Semicolon()}"; break;
                case LocalFunctionStatementSyntax node:
                    value = $"{GetSign(node, opt)}{GetBodyOrExpression(node, opt)}{Semicolon()}"; break;
                case PropertyDeclarationSyntax node:
                    value = $"{ToCode(node.Type, opt)} {ToCode(node.ExplicitInterfaceSpecifier, opt)}{node.Identifier} {ToCode(node.AccessorList, opt)}";

                    if (null != node.Initializer)
                    {
                        value = $"{value} {ToCode(node.Initializer, opt)}";
                    }

                    value = $"{value}{Semicolon()}";

                    if (node.Modifiers.Any())
                    {
                        value = $"{node.Modifiers} {value}";
                    }
                    break;
                case AccessorListSyntax node:
                    value = $"{node.OpenBraceToken} {string.Join(" ", node.Accessors.Select(c => ToCode(c, opt)))} {node.CloseBraceToken}";
                    break;
                case AccessorDeclarationSyntax node:
                    value = $"{node.Keyword}";

                    if (null != node.ExpressionBody)
                    {
                        value = $"{value}{ToCode(node.ExpressionBody, opt)}";
                    }
                    else if (null != node.Body)
                    {
                        value = $"{value} {ToCode(node.Body, opt)}";
                    }

                    value = $"{value}{Semicolon()}";

                    if (node.Modifiers.Any())
                    {
                        value = $"{node.Modifiers} {value}";
                    }

                    break;
                case FieldDeclarationSyntax node:
                    value = $"{ToCode(node.Declaration, opt)}{Semicolon()}";

                    if (node.Modifiers.Any())
                    {
                        value = $"{node.Modifiers} {value}";
                    }
                    break;
                case TryStatementSyntax node:
                    value = $"{node.TryKeyword} {ToCode(node.Block, opt)} {string.Join(" ", node.Catches.Select(c => ToCode(c, opt)))}";

                    if (null != node.Finally)
                    {
                        value = $"{value} {ToCode(node.Finally, opt)}";
                    }

                    break;
                case ThrowExpressionSyntax node:
                    value = $"{node.ThrowKeyword} {ToCode(node.Expression, opt)}"; break;
                case ThrowStatementSyntax node:
                    value = $"{node.ThrowKeyword} {ToCode(node.Expression, opt)}{node.SemicolonToken}"; break;
                case CatchClauseSyntax node:
                    value = $"{node.CatchKeyword}";

                    if (null != node.Declaration)
                    {
                        value = $"{value} {ToCode(node.Declaration, opt)}";
                    }
                    value = $"{value} {ToCode(node.Block, opt)}";

                    break;
                case CatchDeclarationSyntax node:
                    value = $"{node.OpenParenToken}{ToCode(node.Type, opt)} {node.Identifier}{node.CloseParenToken}"; break;
                //case CatchDeclarationSyntax node:
                //    value = null;
                //case CatchFilterClauseSyntax node:
                //    value = null;
                case FinallyClauseSyntax node:
                    value = $"{node.FinallyKeyword} {ToCode(node.Block, opt)}"; break;
                //case InterpolatedStringTextSyntax node: value = $"{node.TextToken}";
                case InterpolatedStringExpressionSyntax node:
                    value = $"{node.StringStartToken}{string.Join(string.Empty, node.Contents.Select(c => ToCode(c, opt)))}{node.StringEndToken}"; break;
                case InterpolationSyntax node:
                    //string.Format("{0}{1}", "aaa", "bbb");
                    //node.AlignmentClause node.FormatClause ??
                    //{expression[,alignment][:formatString]}
                    value = $"{node.OpenBraceToken}{ToCode(node.Expression, opt)}";

                    if (null != node.AlignmentClause)
                    {
                        value = $"{value}{ToCode(node.AlignmentClause, opt)}";
                    }
                    if (null != node.FormatClause)
                    {
                        value = $"{value}{ToCode(node.FormatClause, opt)}";
                    }

                    value = $"{value}{node.CloseBraceToken}";
                    break;
                case InterpolationAlignmentClauseSyntax node:
                    value = $"{node.CommaToken}{ToCode(node.Value, opt)}"; break;
                case InterpolationFormatClauseSyntax node:
                    value = $"{node.ColonToken}{node.FormatStringToken}"; break;
                case TupleElementSyntax node:
                    value = ToCode(node.Type, opt);

                    if (default != node.Identifier)
                    {
                        value = $"{value} {node.Identifier}";
                    }
                    break;
                case TupleExpressionSyntax node:
                    value = $"{node.OpenParenToken}{string.Join(", ", node.Arguments.Select(c => ToCode(c, opt)))}{node.CloseParenToken}"; break;
                case TupleTypeSyntax node:
                    value = $"{node.OpenParenToken}{string.Join(", ", node.Elements.Select(c => ToCode(c, opt)))}{node.CloseParenToken}"; break;
                case ClassOrStructConstraintSyntax node: value = $"{node.ClassOrStructKeyword}{node.QuestionToken}"; break;
                case TypeConstraintSyntax node: value = $"{ToCode(node.Type, opt)}"; break;
                case ConstructorConstraintSyntax node: value = $"{node.NewKeyword}{node.OpenParenToken}{node.CloseParenToken}"; break;
                case TypeParameterConstraintClauseSyntax node:
                    value = $"{node.WhereKeyword} {ToCode(node.Name, opt)} {node.ColonToken} {string.Join(", ", node.Constraints.Select(c => ToCode(c, opt)))}"; break;
                case CastExpressionSyntax node:
                    value = $"{node.OpenParenToken}{ToCode(node.Type, opt)}{node.CloseParenToken}{ToCode(node.Expression, opt)}"; break;
                case ParenthesizedExpressionSyntax node:
                    value = $"{node.OpenParenToken}{ToCode(node.Expression, opt)}{node.CloseParenToken}"; break;
                case CaseSwitchLabelSyntax node:
                    value = $"{node.Keyword} {ToCode(node.Value, opt)}{node.ColonToken}"; break;
                //case SwitchLabelSyntax node:
                //    value = $"{node.Keyword} {ToCode(node.l, opt)}{node.ColonToken}"; break;
                case CasePatternSwitchLabelSyntax node:
                    value = $"{node.Keyword} {ToCode(node.Pattern, opt)}";
                    if (null != node.WhenClause)
                    {
                        value = $"{value} {ToCode(node.WhenClause, opt)}";
                    }
                    value = $"{value}{node.ColonToken}";
                    break;
                case SwitchSectionSyntax node:
                    value = $"{string.Join(" ", node.Labels.Select(c => ToCode(c, opt)))} {string.Join(newLine, node.Statements.Select(c => ToCode(c, opt)))}";
                    break;
                case BreakStatementSyntax node: value = $"{node.BreakKeyword}{Semicolon()}"; break;
                //ExpressionStatementSyntax
                case SwitchStatementSyntax node:
                    value = $"{node.SwitchKeyword} {node.OpenParenToken}{ToCode(node.Expression, opt)}{node.CloseParenToken}{newLine}{node.OpenBraceToken}{newLine}{string.Join(newLine, node.Sections.Select(c => ToCode(c, opt)))}{newLine}{node.CloseBraceToken}"; break;
                case ForEachStatementSyntax node:
                    value = $"{node.ForEachKeyword} {node.OpenParenToken}{ToCode(node.Type, opt)} {node.Identifier} {node.InKeyword} {ToCode(node.Expression, opt)}{node.CloseParenToken} {ToCode(node.Statement, opt)}"; break;
                case ForStatementSyntax node:
                    value = $"{node.ForKeyword} {node.OpenParenToken}{ToCode(node.Declaration, opt)}{node.FirstSemicolonToken} {ToCode(node.Condition, opt)}{node.FirstSemicolonToken} {string.Join(" ", node.Incrementors.Select(c => ToCode(c, opt)))}{node.CloseParenToken} {ToCode(node.Statement, opt)}"; break;
                case PostfixUnaryExpressionSyntax node: value = $"{node.Operand}{node.OperatorToken}"; break;
                case PrefixUnaryExpressionSyntax node: value = $"{node.OperatorToken}{node.Operand}"; break;
                case ConditionalAccessExpressionSyntax node:
                    value = $"{ToCode(node.Expression, opt)}{node.OperatorToken}{ToCode(node.WhenNotNull, opt)}"; break;
                case ConditionalExpressionSyntax node:
                    value = $"{ToCode(node.Condition, opt)} {node.QuestionToken} {node.WhenTrue} {node.ColonToken} {node.WhenFalse}"; break;
                case IfStatementSyntax node:
                    value = $"{node.IfKeyword} {node.OpenParenToken}{ToCode(node.Condition, opt)}{node.CloseParenToken}{(node.Statement is BlockSyntax ? string.Empty : " ")}{ToCode(node.Statement, opt)}";

                    if (null != node.Else)
                    {
                        value = $"{value} {ToCode(node.Else, opt)}";
                    }
                    break;
                case ElseClauseSyntax node:
                    value = $"{node.ElseKeyword}{(node.Statement is BlockSyntax ? string.Empty : " ")}{ToCode(node.Statement, opt)}";
                    break;
                case BinaryExpressionSyntax node:
                    value = $"{ToCode(node.Left, opt)} {node.OperatorToken} {ToCode(node.Right, opt)}"; break;
                case AssignmentExpressionSyntax node:
                    value = $"{ToCode(node.Left, opt)} {node.OperatorToken} {ToCode(node.Right, opt)}"; break;
                case QualifiedNameSyntax node:
                    value = $"{ToCode(node.Left, opt)}{node.DotToken}{ToCode(node.Right, opt)}"; break;
                case NullableTypeSyntax node:
                    value = $"{ToCode(node.ElementType, opt)}{node.QuestionToken}"; break;
                case TypeArgumentListSyntax node:
                    value = $"{node.LessThanToken}{string.Join(", ", node.Arguments.Select(c => ToCode(c, opt)))}{node.GreaterThanToken}"; break;
                case TypeParameterListSyntax node:
                    if (node.Parameters.Any())
                    {
                        value = $"{node.LessThanToken}{string.Join(", ", node.Parameters.Select(c => ToCode(c, opt)))}{node.GreaterThanToken}";
                    }
                    break;
                case ArgumentListSyntax node:
                    value = $"{node.OpenParenToken}{string.Join(", ", node.Arguments.Select(c => ToCode(c, opt)))}{node.CloseParenToken}"; break;
                case ParenthesizedLambdaExpressionSyntax node:
                    {
                        value = $"{ToCode(node.ParameterList, opt)} {node.ArrowToken} {ToCode(node.Body, opt)}";

                        if (!node.AsyncKeyword.IsKind(SyntaxKind.None))
                        //if (null != node.AsyncKeyword)
                        {
                            value = $"{node.AsyncKeyword} {value}";
                        }
                    }
                    break;
                case ParameterListSyntax node:
                    value = $"{node.OpenParenToken}{string.Join(", ", node.Parameters.Select(c => ToCode(c, opt)))}{node.CloseParenToken}"; break;
                //case PredefinedTypeSyntax node:
                case ArrayRankSpecifierSyntax node:
                    value = $"{node.OpenBracketToken}{node.Sizes}{node.CloseBracketToken}"; break;
                case ArrayTypeSyntax node:
                    {
                        var array = node.RankSpecifiers.FirstOrDefault();

                        value = $"{ToCode(node.ElementType, opt)}{(null == array ? null : ToCode(array, opt))}";
                    }
                    break;
                case ArrayCreationExpressionSyntax node:
                    value = $"{node.NewKeyword} {ToCode(node.Type, opt)} {ToCode(node.Initializer, opt)}"; break;
                case ParameterSyntax node:
                    {
                        value = node.Identifier.ToString();

                        if (null != node.Type)
                        {
                            value = $"{ToCode(node.Type, opt)} {value}";
                        }

                        if (node.Modifiers.Any())
                        {
                            value = $"{node.Modifiers} {value}";
                        }

                        if (null != node.Default)
                        {
                            value = $"{value} {ToCode(node.Default, opt)}";
                        }
                    }
                    break;
                case ArgumentSyntax node:
                    {
                        value = $"{ToCode(node.Expression, opt)}";

                        //if (null != node.NameColon)
                        //{
                        //    value = $"{ToCode(node.NameColon, opt)} {value}";
                        //}

                        if (default == node.RefOrOutKeyword)
                        {
                            if (null != node.NameColon)
                            {
                                value = $"{ToCode(node.NameColon, opt)} {value}";
                            }
                        }
                        else
                        {
                            if (null != node.NameColon)
                            {
                                value = $"{value} {node.NameColon.Name}";
                            }

                            value = $"{node.RefOrOutKeyword} {value}";
                        }
                    }
                    break;
                case NameColonSyntax node:
                    value = $"{ToCode(node.Name, opt)}{node.ColonToken}"; break;
                case ElementAccessExpressionSyntax node:
                    value = $"{ToCode(node.Expression, opt)}{ToCode(node.ArgumentList, opt)}"; break;
                case BracketedArgumentListSyntax node:
                    value = $"{node.OpenBracketToken}{string.Join(", ", node.Arguments.Select(c => ToCode(c, opt)))}{node.CloseBracketToken}"; break;
                case EqualsValueClauseSyntax node:
                    value = $"{node.EqualsToken} {ToCode(node.Value, opt)}"; break;
                case MemberAccessExpressionSyntax node:
                    value = $"{node.Expression}{node.OperatorToken}{node.Name}"; break;
                case MemberBindingExpressionSyntax node:
                    value = $"{node.OperatorToken}{node.Name}"; break;
                case ExplicitInterfaceSpecifierSyntax node:
                    value = $"{ToCode(node.Name, opt)}{node.DotToken}"; break;
                case InvocationExpressionSyntax node:
                    value = $"{node.Expression}{node.ArgumentList.OpenParenToken}";
                    if (0 < node.ArgumentList.Arguments.Count)
                    {
                        value = $"{value}{string.Join(", ", node.ArgumentList.Arguments.Select(c => ToCode(c, opt)))}";
                    }
                    value = $"{value}{node.ArgumentList.CloseParenToken}";
                    break;
                case ObjectCreationExpressionSyntax node:
                    {
                        value = $"{node.NewKeyword} {ToCode(node.Type, opt)}";

                        if (null != node.ArgumentList)
                        {
                            value = $"{value}{node.ArgumentList.OpenParenToken}";

                            if (0 < node.ArgumentList.Arguments.Count)
                            {
                                value = $"{value}{string.Join(", ", node.ArgumentList.Arguments.Select(c => ToCode(c, opt)))}";
                            }

                            value = $"{value}{node.ArgumentList.CloseParenToken}";
                        }
                        else if (null != node.Initializer)
                        {
                            value = $"{value} {ToCode(node.Initializer, opt)}";
                        }
                    }
                    break;
                case InitializerExpressionSyntax node:
                    value = node.OpenBraceToken.ToString();

                    if (0 < node.Expressions.Count)
                    {
                        value = $"{value} {string.Join(", ", node.Expressions.Select(c => ToCode(c, opt)))}";
                    }

                    value = $"{value} {node.CloseBraceToken}";

                    break;
                case BlockSyntax node:
                    value = $"{newLine}{node.OpenBraceToken}";

                    if (node.Statements.Any())
                    {
                        value = $"{value}{newLine}{string.Join(newLine2, node.Statements.Select(c => ToCode(c, opt)))}";
                    }

                    value = $"{value}{newLine}{node.CloseBraceToken}";

                    break;
                case SimpleLambdaExpressionSyntax node:
                    value = $"{node.Parameter} {node.ArrowToken} {ToCode(node.Body, opt)}"; break;
                case AwaitExpressionSyntax node:
                    value = $"{node.AwaitKeyword} {ToCode(node.Expression, opt)}"; break;
                case ArrowExpressionClauseSyntax node:
                    value = $" {node.ArrowToken} {ToCode(node.Expression, opt)}"; break;
                case TypeOfExpressionSyntax node:
                    value = $"{node.Keyword}{node.OpenParenToken}{ToCode(node.Type, opt)}{node.CloseParenToken}"; break;
                case VariableDeclaratorSyntax node:
                    value = $"{node.Identifier}";

                    if (null != node.Initializer)
                    {
                        value = $"{value} {ToCode(node.Initializer, opt)}";
                    }
                    if (null != node.ArgumentList)
                    {
                        value = $"{value}{ToCode(node.ArgumentList, opt)}";
                    }

                    break;
                case GenericNameSyntax node:
                    value = $"{node.Identifier}";
                    //value = $"{GetPrefix(node)}";

                    if (node.TypeArgumentList.Arguments.Any())
                    {
                        value = $"{value}{ToCode(node.TypeArgumentList, opt)}";
                    }

                    //if (node.IsUnboundGenericName)??
                    //{

                    //}

                    break;
                //IsPatternExpressionSyntax IsPatternExpression 
                case IsPatternExpressionSyntax node:
                    value = $"{ToCode(node.Expression, opt)} {node.IsKeyword} {ToCode(node.Pattern, opt)}"; break;
                case PointerTypeSyntax node:
                    value = $"{ToCode(node.ElementType, opt)}{node.AsteriskToken}"; break;
                #endregion

                #region default

                default:
                    if (syntaxNode.ChildNodes().Any())
                    {
                        switch (syntaxNode)
                        {
                            //ExplicitInterfaceSpecifier = ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier
                            //ArrowExpressionClauseSyntax
                            //case MethodDeclarationSyntax node:
                            //    value = $"{node.Identifier} "; break;
                            //case ArrowExpressionClauseSyntax node:
                            //    value = $"{node.ArrowToken} "; break;
                            //case ExplicitInterfaceSpecifierSyntax node:
                            //    code = $"{node.Name}{node.DotToken}"; break;
                            //case SimpleLambdaExpressionSyntax node:
                            //    return $"{node.Parameter} {node.ArrowToken}";
                            //case ThrowStatementSyntax node:
                            //    value = $"{node.ThrowKeyword} "; break;
                            //case ReturnStatementSyntax node:
                            //    value = $"{node.ReturnKeyword} "; break;
                            //case ObjectCreationExpressionSyntax node:
                            //    code = $"{node.NewKeyword} "; break;
                            //case GenericNameSyntax node:
                            //    value = node.Identifier.ToString(); break;
                            case IdentifierNameSyntax node:
                                value = $"{node.Identifier} "; break;
                            //value = $"{GetPrefix(node)} "; break;
                            //GetPrefix
                            //case VariableDeclaratorSyntax node:
                            //    value = $"{node.Identifier} "; break;
                            //case EqualsValueClauseSyntax node:
                            //    code = $"{node.EqualsToken} "; break;
                            default: break;
                        }

                        value = $"{value}{string.Join(" ", syntaxNode.ChildNodes().Select(c => ToCode(c, opt)))}{Semicolon()}";
                    }
                    else
                    {
                        value = $"{syntaxNode}";
                    }

                    break;

                    #endregion
            }

            if (null != opt.Replace)
            {
                return opt.Replace(syntaxNode, value);
            }

            return value;

            SyntaxToken Semicolon()
            {
                switch (syntaxNode)
                {
                    case ClassDeclarationSyntax node:
                        return node.SemicolonToken;
                    case FieldDeclarationSyntax node:
                        return node.SemicolonToken;
                    case PropertyDeclarationSyntax node:
                        return node.SemicolonToken;
                    case AccessorDeclarationSyntax node:
                        return node.SemicolonToken;
                    case MethodDeclarationSyntax node:
                        return node.SemicolonToken;
                    case LocalDeclarationStatementSyntax node:
                        return node.SemicolonToken;
                    case ReturnStatementSyntax node:
                        return node.SemicolonToken;
                    case ExpressionStatementSyntax node:
                        return node.SemicolonToken;
                    case BreakStatementSyntax node:
                        return node.SemicolonToken;
                    default: return default;
                }
            }

            //string GetPrefix(SyntaxNode node)
            //{
            //    //GenericNameSyntax
            //    var name = node.GetFullName();
            //    if (!DeclaredSymbols.TryGetValue(name, out SymbolInfo targetInfo))
            //    {
            //        if (!TypeSymbols.TryGetValue(name, out ITypeSymbol genericType2))
            //        {
            //            //break;
            //        }

            //        return null;
            //    }

            //    var name2 = targetInfo.Declared.GetFullName();

            //    return name2;
            //}
        }
    }
}
