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
    using System.Collections.Generic;
    using System.Linq;
    using TypeNameFormatter;

    public static class SyntaxFactoryExt
    {
        public static ObjectCreationExpressionSyntax ParseObjectCreation(TypeSyntax type, ArgumentListSyntax? argumentList, InitializerExpressionSyntax? initializer)
        {
            return SyntaxFactory.ObjectCreationExpression(type, argumentList, initializer);
        }

        public static InitializerExpressionSyntax ParseElementInitializer(ExpressionSyntax key, ExpressionSyntax value)
        {
            return SyntaxFactory.InitializerExpression(SyntaxKind.ComplexElementInitializerExpression, new SeparatedSyntaxList<ExpressionSyntax>().AddRange(new ExpressionSyntax[] { key, value }));
        }
        public static InitializerExpressionSyntax ParseElementInitializer(params (ExpressionSyntax key, ExpressionSyntax value)[] value)
        {
            return SyntaxFactory.InitializerExpression(SyntaxKind.ComplexElementInitializerExpression, new SeparatedSyntaxList<ExpressionSyntax>().AddRange(value.Select(c => ParseElementInitializer(c.key, c.value))));
        }

        //public static SeparatedSyntaxList<ExpressionSyntax> ParseElementInitializer(ExpressionSyntax key, ExpressionSyntax value)
        //{
        //    var element = SyntaxFactory.InitializerExpression(SyntaxKind.ComplexElementInitializerExpression, new SeparatedSyntaxList<ExpressionSyntax>().AddRange(new ExpressionSyntax[] { key, value }));
        //    return new SeparatedSyntaxList<ExpressionSyntax>().Add(element);
        //}

        public static TypeSyntax ParseType(this Type type) => SyntaxFactory.ParseTypeName(type.GetFormattedName());

        /*
        public static TypeSyntax ParseType(this string name)
        {
            var spKey = '.';
            var genericKey = '<';
            var genericIdx = name.IndexOf(genericKey);
            var isGeneric = -1 < genericIdx;
            var name2 = isGeneric ? name.Substring(0, genericIdx) : name;
            var sp = new ReadOnlySpan<string>(name2.Split(spKey));
            var isQualified = 1 < sp.Length;
            var leftStr = sp.Slice(0, sp.Length - 1).ToString(spKey);
            var rightStr = sp.Slice(sp.Length - 1).ToString(spKey);
            var genericStr = isGeneric ? name.Substring(genericIdx + 1, name.Length - genericIdx - 2) : string.Empty;

            if (isGeneric)
            {
                var generics = GetGenerics(genericStr);

                var args = new SeparatedSyntaxList<TypeSyntax>();

                foreach (var item in generics)
                {
                    args = args.Add(ParseType(item));
                }

                if (string.IsNullOrEmpty(leftStr))
                {
                    return SyntaxFactory.GenericName(SyntaxFactory.ParseToken(rightStr), SyntaxFactory.TypeArgumentList(args));
                }

                return SyntaxFactory.QualifiedName(SyntaxFactory.ParseName(leftStr), SyntaxFactory.GenericName(SyntaxFactory.ParseToken(rightStr), SyntaxFactory.TypeArgumentList(args)));
            }
            else if (isQualified)
            {
                if (string.IsNullOrEmpty(leftStr))
                {
                    return SyntaxFactory.IdentifierName(rightStr);
                }

                return SyntaxFactory.QualifiedName(SyntaxFactory.ParseName(leftStr), SyntaxFactory.IdentifierName(rightStr));
            }
            else
            {
                return SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(SyntaxTriviaList.Empty, SyntaxKind.TypeKeyword, name2, name2, SyntaxTriviaList.Empty));
            }

            IEnumerable<string> GetGenerics(string genericStr)
            {
                var result = new List<string>();

                var sp = new ReadOnlySpan<char>(genericStr.ToArray());

                var index = 0;
                var l = 0;
                for (int i = 0; i < sp.Length; i++)
                {
                    var c = sp[i];

                    switch (c)
                    {
                        case ',':
                            if (0 < l) { break; }
                            result.Add(new string(sp.Slice(index, i).ToArray()).Trim());
                            index = i + 1;
                            break;
                        case '<': l++; break;
                        case '>': l--; break;
                        default: break;
                    }

                    if (0 < index && i == sp.Length - 1)
                    {
                        result.Add(new string(sp.Slice(index).ToArray()).Trim());
                    }
                }

                if (0 < result.Count)
                {
                    return result;
                }

                return Enumerable.Empty<string>();
            }
        }
        */

        public static ArgumentSyntax ParseElementArgument(TypeSyntax type, params (ExpressionSyntax key, ExpressionSyntax value)[] value) => SyntaxFactory.Argument(ParseObjectCreation(type, null, ParseElementInitializer(value)));

        public static TypeArgumentListSyntax TypeArgumentList(params TypeSyntax[] arg)
        {
            if (!(arg?.Any() ?? false)) { return SyntaxFactory.TypeArgumentList(); }

            return SyntaxFactory.TypeArgumentList().AddArguments(arg);
        }

        public static TypeArgumentListSyntax Add(this TypeArgumentListSyntax list, params TypeSyntax[] arg)
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (!(arg?.Any() ?? false)) { return list; }

            return list.AddArguments(arg);
        }

        public static ArgumentListSyntax ArgumentList(params ExpressionSyntax[] arg)
        {
            if (!(arg?.Any() ?? false)) { return SyntaxFactory.ArgumentList(); }

            return SyntaxFactory.ArgumentList().AddArguments(arg.Select(c => SyntaxFactory.Argument(c)).ToArray());
        }

        public static ArgumentListSyntax Add(this ArgumentListSyntax list, params ExpressionSyntax[] arg)
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (!(arg?.Any() ?? false)) { return list; }

            return list.AddArguments(arg.Select(c => SyntaxFactory.Argument(c)).ToArray());
        }

        /// <summary>
        /// Creates a new TypeOfExpressionSyntax instance.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TypeOfExpressionSyntax TypeOfExpression(Type type) => SyntaxFactory.TypeOfExpression(ParseType(type));

        public static InvocationExpressionSyntax NameOf(ExpressionSyntax arg) => InvocationExpression(SyntaxFactory.IdentifierName("nameof"), arg);
        public static IdentifierNameSyntax ArgumentNullException() => SyntaxFactory.IdentifierName("ArgumentNullException");

        public static InitializerExpressionSyntax ArrayInitializerExpression(params ExpressionSyntax[] expressions) => SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression, new SeparatedSyntaxList<ExpressionSyntax>().AddRange(expressions));

        public static ObjectCreationExpressionSyntax ObjectCreationExpression(TypeSyntax type, params ExpressionSyntax[] arg) => SyntaxFactory.ObjectCreationExpression(type, ArgumentList(arg), null);

        #region ParseToken

        public static SyntaxTokenList ParseTokenList(params SyntaxKind[] modifiers) => new SyntaxTokenList(ParseSyntaxToken(modifiers));

        public static SyntaxToken[] ParseTokenArray(params SyntaxKind[] modifiers) => ParseSyntaxToken(modifiers).ToArray();

        public static IEnumerable<SyntaxToken> ParseSyntaxToken(params SyntaxKind[] modifiers) => modifiers.Select(c => SyntaxFactory.Token(SyntaxTriviaList.Empty, c, new SyntaxTriviaList(SyntaxFactory.ParseTrailingTrivia(" "))));

        #endregion

        public static Member AddModifiers<Member>(this Member classd, params SyntaxKind[] modifiers) where Member : MemberDeclarationSyntax => classd.AddModifiers(ParseTokenArray(modifiers)) as Member;

        public static SeparatedSyntaxList<TypeParameterSyntax> ParseTypeParameterList(params string[] name) => new SeparatedSyntaxList<TypeParameterSyntax>().AddRange(name.Select(c => SyntaxFactory.TypeParameter(c)));

        public static SyntaxToken ParseSemicolonToken() => Token(SyntaxKind.SemicolonToken, ";");

        public static SyntaxToken Token(SyntaxKind kind, string text) => Token(kind, text, nameof(String));
        public static SyntaxToken Token(SyntaxKind kind, string text, string valueText) => SyntaxFactory.Token(SyntaxTriviaList.Empty, kind, text, valueText, SyntaxTriviaList.Empty);

        public static MemberAccessExpressionSyntax MemberAccessExpression(string left, string right) => MemberAccessExpression(SyntaxFactory.IdentifierName(left), SyntaxFactory.IdentifierName(right));
        public static MemberAccessExpressionSyntax MemberAccessExpression(ExpressionSyntax left, string right) => MemberAccessExpression(left, SyntaxFactory.IdentifierName(right));
        public static MemberAccessExpressionSyntax MemberAccessExpression(string left, SimpleNameSyntax right) => MemberAccessExpression(SyntaxFactory.IdentifierName(left), right);
        public static MemberAccessExpressionSyntax MemberAccessExpression(ExpressionSyntax left, SimpleNameSyntax right) => SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, left, right);

        public static MemberAccessExpressionSyntax MemberAccessExpression(string left, params string[] right)
        {
            if (left is null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            MemberAccessExpressionSyntax memberAccessExpression = null;

            foreach (var item in right)
            {
                if (null == memberAccessExpression)
                {
                    memberAccessExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(left), SyntaxFactory.IdentifierName(item));
                }
                else
                {
                    memberAccessExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, memberAccessExpression, SyntaxFactory.IdentifierName(item));
                }
            }

            return memberAccessExpression;
        }

        public static VariableDeclarationSyntax VariableDeclaration(string type, params VariableDeclaratorSyntax[] variables) => SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(type), new SeparatedSyntaxList<VariableDeclaratorSyntax>().AddRange(variables));
        public static VariableDeclaratorSyntax VariableDeclarator(string identifier, ExpressionSyntax initializer) => SyntaxFactory.VariableDeclarator(identifier).WithInitializer(SyntaxFactory.EqualsValueClause(initializer));
        public static VariableDeclaratorSyntax VariableDeclarator(SyntaxToken identifier, ExpressionSyntax initializer) => SyntaxFactory.VariableDeclarator(identifier).WithInitializer(SyntaxFactory.EqualsValueClause(initializer));

        public static InvocationExpressionSyntax InvocationExpression(ExpressionSyntax expression, params ExpressionSyntax[] arg) => SyntaxFactory.InvocationExpression(expression, ArgumentList(arg));

        public static QualifiedNameSyntax QualifiedName(string left, string right) => SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName(left), SyntaxFactory.IdentifierName(right));

        public static QualifiedNameSyntax QualifiedName(string left, params string[] right)
        {
            if (left is null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            QualifiedNameSyntax qualifiedName = null;

            foreach (var item in right)
            {
                if (null == qualifiedName)
                {
                    qualifiedName = SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName(left), SyntaxFactory.IdentifierName(item));
                }
                else
                {
                    qualifiedName = SyntaxFactory.QualifiedName(qualifiedName, SyntaxFactory.IdentifierName(item));
                }
            }

            return qualifiedName;
        }

        public static QualifiedNameSyntax QualifiedName(NameSyntax left, string right) => SyntaxFactory.QualifiedName(left, SyntaxFactory.IdentifierName(right));

        public static QualifiedNameSyntax QualifiedName(string left, SimpleNameSyntax right) => SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName(left), right);

        public static QualifiedNameSyntax QualifiedName(NameSyntax left, SimpleNameSyntax right) => SyntaxFactory.QualifiedName(left, right);

        public static BinaryExpressionSyntax BinaryExpression(ExpressionSyntax left, params ExpressionSyntax[] right)
        {
            if (left is null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            BinaryExpressionSyntax binaryExpression = null;

            foreach (var item in right)
            {
                if (null == binaryExpression)
                {
                    binaryExpression = SyntaxFactory.BinaryExpression(SyntaxKind.BitwiseOrExpression, left, item);
                }
                else
                {
                    binaryExpression = SyntaxFactory.BinaryExpression(SyntaxKind.BitwiseOrExpression, binaryExpression, item);
                }
            }

            return binaryExpression;
        }

        #region namespace

        public static NamespaceDeclarationSyntax ParseNamespace(string name) => SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(name));

        public static NamespaceDeclarationSyntax AddUsings(this NamespaceDeclarationSyntax namespaced, params string[] usings) => namespaced.AddUsings(usings.Select(c => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(c))).ToArray());

        #endregion

        #region ParseClass

        public static ClassDeclarationSyntax ParseClass(string name) => SyntaxFactory.ClassDeclaration(name);

        public static ClassDeclarationSyntax AddBaseListTypes(this ClassDeclarationSyntax classd, params TypeSyntax[] types) => classd.AddBaseListTypes(types.Select(c => SyntaxFactory.SimpleBaseType(c)).ToArray());

        //SyntaxFactory.ClassDeclaration(null, ParseTokenList(modifiers), SyntaxFactory.Identifier(name), ParseTypeParameterList(""), null, null);
        #endregion

        #region Method

        public static MethodDeclarationSyntax ParseMethod(string name) => SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), SyntaxFactory.Identifier(name)).WithBody(SyntaxFactory.Block());

        public static MethodDeclarationSyntax WithReturnType(this MethodDeclarationSyntax method, string returnType) => method.WithReturnType(SyntaxFactory.ParseTypeName(returnType));
        public static MethodDeclarationSyntax WithReturnType(this MethodDeclarationSyntax method, Type returnType) => method.WithReturnType(ParseType(returnType));

        public static MethodDeclarationSyntax WithParameters(this MethodDeclarationSyntax method, params ParameterSyntax[] parameter) => method.WithParameterList(SyntaxFactory.ParseParameterList(string.Empty).WithOpenParenToken(SyntaxFactory.Token(SyntaxKind.OpenParenToken)).WithCloseParenToken(SyntaxFactory.Token(SyntaxKind.CloseParenToken)).AddParameters(parameter));

        #region Body

        public static MethodDeclarationSyntax WithBodyDefault(this MethodDeclarationSyntax method) => method.WithBody(SyntaxFactory.Block(SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression))));

        public static MethodDeclarationSyntax WithExpressionBodyDefault(this MethodDeclarationSyntax method) => method.WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression))).WithSemicolonToken(ParseSemicolonToken());

        public static MethodDeclarationSyntax WithBody(this MethodDeclarationSyntax method, params StatementSyntax[] statements) => method.WithBody(SyntaxFactory.Block(statements ?? default));

        public static MethodDeclarationSyntax WithBody(this MethodDeclarationSyntax method, IEnumerable<StatementSyntax> statements) => method.WithBody(SyntaxFactory.Block(statements ?? default));

        public static MethodDeclarationSyntax WithBody(this MethodDeclarationSyntax method, SyntaxList<StatementSyntax> statements = default) => method.WithBody(SyntaxFactory.Block(statements));

        public static MethodDeclarationSyntax WithBody(this MethodDeclarationSyntax method, SyntaxList<AttributeListSyntax> attributeLists, SyntaxList<StatementSyntax> statements) => method.WithBody(SyntaxFactory.Block(attributeLists, statements));

        public static MethodDeclarationSyntax WithBody(this MethodDeclarationSyntax method, SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken openBraceToken, SyntaxList<StatementSyntax> statements, SyntaxToken closeBraceToken) => method.WithBody(SyntaxFactory.Block(attributeLists, openBraceToken, statements, closeBraceToken));

        #endregion

        #endregion

        #region ParseSwitch

        /// <summary>
        /// Creates a new SwitchStatementSyntax instance.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="switchSections"></param>
        /// <returns></returns>
        public static SwitchStatementSyntax ParseSwitch(string key, params (SwitchLabelSyntax Case, StatementSyntax Value)[] switchSections) => SyntaxFactory.SwitchStatement(SyntaxFactory.IdentifierName(key), new SyntaxList<SwitchSectionSyntax>(switchSections.Select(c => SyntaxFactory.SwitchSection(new SyntaxList<SwitchLabelSyntax>(c.Case), new SyntaxList<StatementSyntax>(c.Value)))));

        /// <summary>
        /// Creates a new SwitchStatementSyntax instance.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="switchSections"></param>
        /// <returns></returns>
        public static SwitchStatementSyntax ParseSwitch(ExpressionSyntax key, params (SwitchLabelSyntax Case, StatementSyntax Value)[] switchSections) => SyntaxFactory.SwitchStatement(key, new SyntaxList<SwitchSectionSyntax>(switchSections.Select(c => SyntaxFactory.SwitchSection(new SyntaxList<SwitchLabelSyntax>(c.Case), new SyntaxList<StatementSyntax>(c.Value)))));

        /// <summary>
        /// Creates a new SwitchStatementSyntax instance.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultSection"></param>
        /// <param name="switchSections"></param>
        /// <returns></returns>
        public static SwitchStatementSyntax ParseSwitch(string key, StatementSyntax defaultSection, params (ExpressionSyntax Case, StatementSyntax Value)[] switchSections) => SyntaxFactory.SwitchStatement(SyntaxFactory.IdentifierName(key), new SyntaxList<SwitchSectionSyntax>(switchSections.Select(c => SyntaxFactory.SwitchSection(new SyntaxList<SwitchLabelSyntax>(SyntaxFactory.CaseSwitchLabel(c.Case)), new SyntaxList<StatementSyntax>(c.Value)))).Add(SyntaxFactory.SwitchSection(new SyntaxList<SwitchLabelSyntax>(SyntaxFactory.DefaultSwitchLabel()), new SyntaxList<StatementSyntax>(defaultSection))));

        /// <summary>
        /// Creates a new SwitchStatementSyntax instance.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultSection"></param>
        /// <param name="switchSections"></param>
        /// <returns></returns>
        public static SwitchStatementSyntax ParseSwitch(ExpressionSyntax key, StatementSyntax defaultSection, params (ExpressionSyntax Case, StatementSyntax Value)[] switchSections) => SyntaxFactory.SwitchStatement(key, new SyntaxList<SwitchSectionSyntax>(switchSections.Select(c => SyntaxFactory.SwitchSection(new SyntaxList<SwitchLabelSyntax>(SyntaxFactory.CaseSwitchLabel(c.Case)), new SyntaxList<StatementSyntax>(c.Value)))).Add(SyntaxFactory.SwitchSection(new SyntaxList<SwitchLabelSyntax>(SyntaxFactory.DefaultSwitchLabel()), new SyntaxList<StatementSyntax>(defaultSection))));

        #endregion

        #region ParseLiteral

        //public static LiteralExpressionSyntax ParseStringLiteral(string text) => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.StringLiteralToken, $"\"{text}\"", nameof(String), SyntaxTriviaList.Empty));

        public static LiteralExpressionSyntax ParseBooleanLiteral(bool value) => value ? SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression) : SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);

        /// <summary>
        /// Creates a new DefaultLiteralExpression instance.
        /// </summary>
        /// <returns></returns>
        public static LiteralExpressionSyntax ParseDefaultLiteral() => SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression);

        public static LiteralExpressionSyntax ParseLiteralNull() => SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);

        /// <summary>
        /// Creates a new StringLiteralExpression instance.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static LiteralExpressionSyntax ParseLiteral(string value) => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));

        /// <summary>
        /// Creates a new CharacterLiteralExpression instance.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static LiteralExpressionSyntax ParseLiteral(char value) => SyntaxFactory.LiteralExpression(SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal(value));

        public static LiteralExpressionSyntax ParseLiteral(float value) => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));

        public static LiteralExpressionSyntax ParseLiteral(ulong value) => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));

        public static LiteralExpressionSyntax ParseLiteral(long value) => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));

        public static LiteralExpressionSyntax ParseLiteral(uint value) => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));

        public static LiteralExpressionSyntax ParseLiteral(double value) => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));

        public static LiteralExpressionSyntax ParseLiteral(decimal value) => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));

        public static LiteralExpressionSyntax ParseLiteral(int value) => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));

        #endregion

        #region ParseProperty

        public static PropertyDeclarationSyntax ParseProperty(string name, string type, params SyntaxKind[] modifiers) => ParseProperty(name, SyntaxFactory.ParseTypeName(type), null, SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(ParseSemicolonToken()), modifiers);
        public static PropertyDeclarationSyntax ParseProperty(string name, Type type, params SyntaxKind[] modifiers) => ParseProperty(name, ParseType(type), null, SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(ParseSemicolonToken()), modifiers);
        public static PropertyDeclarationSyntax ParseProperty(string name, TypeSyntax type, params SyntaxKind[] modifiers) => ParseProperty(name, type, null, SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(ParseSemicolonToken()), modifiers);

        public static PropertyDeclarationSyntax ParseProperty(string name, string type, AccessorDeclarationSyntax get = null, AccessorDeclarationSyntax set = null, params SyntaxKind[] modifiers) => ParseProperty(name, SyntaxFactory.ParseTypeName(type), get, set, modifiers);
        public static PropertyDeclarationSyntax ParseProperty(string name, Type type, AccessorDeclarationSyntax get = null, AccessorDeclarationSyntax set = null, params SyntaxKind[] modifiers) => ParseProperty(name, ParseType(type), get, set, modifiers);
        public static PropertyDeclarationSyntax ParseProperty(string name, TypeSyntax type, AccessorDeclarationSyntax get = null, AccessorDeclarationSyntax set = null, params SyntaxKind[] modifiers)
        {
            var accessors = SyntaxFactory.AccessorList(new SyntaxList<AccessorDeclarationSyntax>().Add(get ?? SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(ParseSemicolonToken())));

            if (null != set)
            {
                accessors = accessors.AddAccessors(set);
            }

            return SyntaxFactory.PropertyDeclaration(default(SyntaxList<AttributeListSyntax>), ParseTokenList(modifiers), type, default, SyntaxFactory.Identifier(name), accessors);
        }

        public static PropertyDeclarationSyntax Initializer(this PropertyDeclarationSyntax property, ExpressionSyntax initializer) => property.WithInitializer(SyntaxFactory.EqualsValueClause(initializer)).WithSemicolonToken(ParseSemicolonToken());

        public static PropertyDeclarationSyntax Initializer(this PropertyDeclarationSyntax property, string type, ArgumentSyntax argument = null, InitializerExpressionSyntax initializer = null) => Initializer(property, SyntaxFactory.ParseTypeName(type), argument, initializer);
        public static PropertyDeclarationSyntax Initializer(this PropertyDeclarationSyntax property, Type type, ArgumentSyntax argument = null, InitializerExpressionSyntax initializer = null) => Initializer(property, ParseType(type), argument, initializer);
        public static PropertyDeclarationSyntax Initializer(this PropertyDeclarationSyntax property, TypeSyntax type, ArgumentSyntax argument = null, InitializerExpressionSyntax initializer = null) => Initializer(property, ParseObjectCreation(type, SyntaxFactory.ArgumentList(null != argument ? new SeparatedSyntaxList<ArgumentSyntax>().Add(argument) : default), initializer));

        #endregion

        #region ParseField

        public static FieldDeclarationSyntax ParseField(string name, string type, params SyntaxKind[] modifiers) => ParseField(name, SyntaxFactory.ParseTypeName(type), modifiers);
        public static FieldDeclarationSyntax ParseField(string name, Type type, params SyntaxKind[] modifiers) => ParseField(name, ParseType(type), modifiers);
        public static FieldDeclarationSyntax ParseField(string name, TypeSyntax type, params SyntaxKind[] modifiers) => SyntaxFactory.FieldDeclaration(default(SyntaxList<AttributeListSyntax>), ParseTokenList(modifiers), SyntaxFactory.VariableDeclaration(type, new SeparatedSyntaxList<VariableDeclaratorSyntax>().Add(SyntaxFactory.VariableDeclarator(name))));

        public static FieldDeclarationSyntax Initializer(this FieldDeclarationSyntax field, ExpressionSyntax initializer) => field.WithDeclaration(field.Declaration.WithVariables(new SeparatedSyntaxList<VariableDeclaratorSyntax>().Add(field.Declaration.Variables.FirstOrDefault().WithInitializer(SyntaxFactory.EqualsValueClause(initializer)))));


        public static FieldDeclarationSyntax Initializer(this FieldDeclarationSyntax field, string type, ArgumentSyntax argument = null, InitializerExpressionSyntax initializer = null) => Initializer(field, SyntaxFactory.ParseTypeName(type), argument, initializer);
        public static FieldDeclarationSyntax Initializer(this FieldDeclarationSyntax field, Type type, ArgumentSyntax argument = null, InitializerExpressionSyntax initializer = null) => Initializer(field, ParseType(type), argument, initializer);
        public static FieldDeclarationSyntax Initializer(this FieldDeclarationSyntax field, TypeSyntax type, ArgumentSyntax argument = null, InitializerExpressionSyntax initializer = null) => Initializer(field, ParseObjectCreation(type, SyntaxFactory.ArgumentList(null != argument ? new SeparatedSyntaxList<ArgumentSyntax>().Add(argument) : default), initializer));

        #endregion

        //public static FieldDeclarationSyntax ParseArgumentList(TypeSyntax type, InitializerExpressionSyntax? initializer)
        //{
        //    return ParseObjectCreation(type, SyntaxFactory.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>().Add(SyntaxFactory.Argument(arg))), initializer);
        //}

        //public static FieldDeclarationSyntax ObjectCreationElement(this FieldDeclarationSyntax field, TypeSyntax type, ExpressionSyntax arg, InitializerExpressionSyntax? initializer)
        //{
        //    return Initializer(field, ParseObjectCreation(type, SyntaxFactory.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>().Add(SyntaxFactory.Argument(arg))), initializer));
        //}
    }
}
