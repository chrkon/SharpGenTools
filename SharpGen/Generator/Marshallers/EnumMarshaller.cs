﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SharpGen.Model;
using Microsoft.CodeAnalysis.CSharp;

namespace SharpGen.Generator.Marshallers
{
    class EnumMarshaller : MarshallerBase, IMarshaller
    {
        public EnumMarshaller(GlobalNamespaceProvider globalNamespace) : base(globalNamespace)
        {
        }

        public bool CanMarshal(CsMarshalBase csElement)
        {
            return csElement.PublicType is CsEnum;
        }

        public ArgumentSyntax GenerateManagedArgument(CsParameter csElement)
        {
            return GenerateManagedValueTypeArgument(csElement);
        }

        public ParameterSyntax GenerateManagedParameter(CsParameter csElement)
        {
            return GenerateManagedValueTypeParameter(csElement);
        }

        public StatementSyntax GenerateManagedToNative(CsMarshalBase csElement, bool singleStackFrame)
        {
            return null;
        }

        public ArgumentSyntax GenerateNativeArgument(CsMarshalCallableBase csElement)
        {
            var csEnum = (CsEnum)csElement.PublicType;

            if (csElement.PassedByNativeReference)
            {
                if (csElement.IsFixed && !csElement.UsedAsReturn)
                {
                    return Argument(GetMarshalStorageLocation(csElement));
                }
                else
                {
                    return Argument(PrefixUnaryExpression(SyntaxKind.AddressOfExpression,
                                IdentifierName(csElement.Name)));
                }
            }
            else
            {
                return Argument(
                    CheckedExpression(
                        SyntaxKind.UncheckedExpression,
                        CastExpression(
                            ParseTypeName(csEnum.UnderlyingType?.Type.FullName ?? "int"),
                            IdentifierName(csElement.Name))));
            }
        }

        public StatementSyntax GenerateNativeCleanup(CsMarshalBase csElement, bool singleStackFrame)
        {
            return null;
        }

        public StatementSyntax GenerateNativeToManaged(CsMarshalBase csElement, bool singleStackFrame)
        {
            return null;
        }

        public FixedStatementSyntax GeneratePin(CsParameter csElement)
        {
            if (csElement.IsFixed && !csElement.IsUsedAsReturnType)
            {
                return FixedStatement(VariableDeclaration(PointerType(PredefinedType(Token(SyntaxKind.VoidKeyword))),
                    SingletonSeparatedList(
                        VariableDeclarator(GetMarshalStorageLocationIdentifier(csElement)).WithInitializer(EqualsValueClause(
                            PrefixUnaryExpression(SyntaxKind.AddressOfExpression,
                                IdentifierName(csElement.Name))
                            )))), EmptyStatement());
            }
            return null;
        }
    }
}
