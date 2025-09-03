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
    | func                                                                      # FuncType
    | Borrow OpenAngle type CloseAngle                                          # BorrowType
    ;

func
    : Func OpenParen funcParamList CloseParen funcResult?
    ;

gate
    : gateItem+
    ;

gateItem
    : Unstable OpenParen Feature Equal identifier CloseParen                    # FeatureGateUnstable
    | Since OpenParen Version Equal semVersion CloseParen                       # FeatureGateSince
    | Deprecated OpenParen (Feature Equal identifier | Version Equal semVersion)? CloseParen # Feature
    ;

funcParam
    : identifier Colon type
    ;

funcResult
    : (Arrow type (Comma type)*)?
    ;

export
    : gate? Export importExport
    ;

import_
    : gate? Import importExport
    ;

importExport
    : identifier Colon externType
    | packageName Semicolon
    ;

externType
    : func Semicolon
    | interface
    ;

include
    : gate? Include packageName with? Semicolon
    ;

with
    : With OpenCurly (withItem (Comma withItem)*)? CloseCurly
    ;

withItem
    : identifier As identifier
    ;

record
    : gate? Record identifier? OpenCurly (recordDefinition (Comma recordDefinition)*)? Comma? CloseCurly
    ;

recordDefinition
    : identifier Colon type
    ;

enum
    : gate? Enum identifier? OpenCurly (enumItem (Comma enumItem)* Comma?)? CloseCurly
    ;

enumItem
    : gate? identifier
    ;

flags
    : gate? Flags identifier? OpenCurly (flagsItem (Comma flagsItem)*)? Comma? CloseCurly
    ;

flagsItem
    : gate? identifier
    ;

world
    : gate? World identifier OpenCurly worldItem* CloseCurly
    ;

typeAlias
    : gate? Type identifier Equal type Semicolon
    ;

worldItem
    : worldDefinition
    ;

worldDefinition
    : export
    | import_
    | include
    | typeDef
    ;

interface
    : gate? Interface identifier? OpenCurly interfaceDefinition* CloseCurly
    ;

interfaceDefinition
    : gate? identifier Colon type Semicolon
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
    : gate? Use packageName Dot OpenCurly (useItem (Comma useItem)*)? CloseCurly Semicolon
    ;

useItem
    : identifier (As identifier)?
    ;

package
    : Package packageName OpenCurly packageDefinition* CloseCurly
    ;

resource
    : gate? Resource identifier (Semicolon|OpenCurly (resourceMethod Semicolon)* CloseCurly)
    ;

static
    : Static
    ;

constructor
    : Constructor
    ;

resourceMethod
    : gate? constructor OpenParen funcParamList CloseParen  # ResourceConstructor
    | gate? identifier Colon static? type                   # ResourceFunction
    ;

funcParamList
    : (funcParam (Comma funcParam)* Comma?)?
    ;

variant
    : gate? Variant identifier? OpenCurly (variantDefinition (Comma variantDefinition)*)? Comma? CloseCurly
    ;

variantDefinition
    : gate? identifier (OpenParen type CloseParen)?
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