parser grammar WitParser;

options {
    tokenVocab = WitLexer;
}

file
    : filePackage? (fileDefinition Semicolon?)*
    ;

fileDefinition
    : package
    | world
    | typeDef
    ;

type
    : U8                                                                        # U8Type
    | U16                                                                       # U16Type
    | U32                                                                       # U32Type
    | U64                                                                       # U64Type
    | S8                                                                        # S8Type
    | S16                                                                       # S16Type
    | S32                                                                       # S32Type
    | S64                                                                       # S64Type
    | F32                                                                       # F32Type
    | F64                                                                       # F64Type
    | StringType                                                                # StringTypeType
    | Char                                                                      # CharType
    | Bool                                                                      # BoolType
    | identifier                                                                # CustomType
    | packageName                                                               # ExternalType
    | List OpenAngle type CloseAngle                                            # ListType
    | Option OpenAngle type CloseAngle                                          # OptionType
    | Result OpenAngle type Comma type CloseAngle                               # ResultType
    | Result OpenAngle Underscore Comma type CloseAngle                         # ResultNoResultType
    | Result OpenAngle type CloseAngle                                          # ResultNoErrorType
    | Result                                                                    # ResultEmptyType
    | Stream OpenAngle type CloseAngle                                          # StreamType
    | Tuple OpenAngle (type (Comma type)*)? CloseAngle                          # TupleType
    | Func OpenParen (funcParam (Comma funcParam)*)? CloseParen funcResult?     # FuncType
    ;

gate
    : gateItem+
    ;

gateItem
    : Unstable OpenParen Feature Equal identifier CloseParen                    # FeatureGateUnstable
    | Since OpenParen Version Equal semVersion CloseParen                       # FeatureGateSince
    | Deprecated OpenParen (Feature Equal identifier | Since Equal semVersion)? CloseParen # Feature
    ;

funcParam
    : identifier Colon type
    ;

funcResult
    : (Arrow type (Comma type)*)?
    ;

export
    : Export identifier (Colon type)? Semicolon
    ;

import_
    : Import identifier Colon interface Semicolon
    | Import (packageName | identifier (Colon type)?) Semicolon
    ;

include
    : Include (packageName | identifier) with? Semicolon
    ;

with
    : With OpenCurly (withItem (Comma withItem)*)? CloseCurly
    ;

withItem
    : identifier As identifier
    ;

record
    : Record identifier? OpenCurly (recordDefinition (Comma recordDefinition)*)? Comma? CloseCurly
    ;

recordDefinition
    : identifier Colon type
    ;

enum
    : Enum identifier? OpenCurly (identifier (Comma identifier)*)? Comma? CloseCurly
    ;

flags
    : Flags identifier? OpenCurly (identifier (Comma identifier)*)? Comma? CloseCurly
    ;

world
    : gate? World identifier OpenCurly worldItem* CloseCurly
    ;

typeAlias
    : Type identifier Equal type Semicolon
    ;

worldItem
    : gate? worldDefinition
    ;

worldDefinition
    : export
    | import_
    | include
    | typeDef
    ;

interface
    : Interface identifier? OpenCurly interfaceDefinition* CloseCurly
    ;

interfaceDefinition
    : identifier Colon type Semicolon
    | typeDef
    ;

typeDef
    : resource
    | variant
    | record
    | flags
    | enum
    | typeAlias
    | use
    | interface
    ;

use
    : Use packageName Dot OpenCurly (useItem (Comma useItem)*)? CloseCurly Semicolon
    ;

useItem
    : identifier (As identifier)?
    ;

package
    : Package packageName OpenCurly packageDefinition* CloseCurly
    ;

resource
    : Resource identifier (Semicolon|OpenCurly (resourceMethod Semicolon)* CloseCurly)
    ;

static
    : Static
    ;

resourceMethod
    : gate? Constructor identifier OpenParen (funcParam (Comma funcParam)*)? CloseParen  # ResourceConstructor
    | gate? identifier static? Colon type                                                # ResourceFunction
    ;

variant
    : Variant identifier? OpenCurly (variantDefinition (Comma variantDefinition)*)? Comma? CloseCurly
    ;

variantDefinition
    : identifier (OpenParen type CloseParen)?
    ;

packageDefinition
    : world
    | typeDef
    ;

filePackage
    : Package packageName Semicolon
    ;

semVersionCore
    : integer (Dot integer)? (Dot integer)?
    ;

integer
    : Integer
    ;

semVersionPreRelase
    : (Dot|Identifier|Integer)*
    ;

semVersionBuild
    : (Dot|Identifier|Integer)*
    ;

semversionExtra
    : (Dash semVersionPreRelase)? (Plus semVersionBuild)?
    ;

semVersion
    : semVersionCore semversionExtra
    ;

identifier
    : Identifier
    | Version
    | Feature
    ;

packageNamespace
    : identifier Colon (identifier Colon)*
    ;

packageName
    : packageNamespace? identifier (Slash identifier)* (At semVersion)?
    ;