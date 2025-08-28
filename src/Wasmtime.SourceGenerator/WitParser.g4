parser grammar WitParser;

options {
    tokenVocab = WitLexer;
}

file
    : filePackage? fileDefinition*
    ;

fileDefinition
    : package
    | world
    | interface
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
    | Identifier                                                                # CustomType
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

funcParam
    : identifier Colon type
    ;

funcResult
    : (Arrow type (Comma type)*)?
    ;

export
    : Export identifier (Colon type)? Semicolon
    ;

externalImportName
    : packageName Slash identifier
    ;

import_
    : Import identifier Colon interface
    | Import (externalImportName | identifier (Colon type)?) Semicolon
    ;

include
    : Include (externalImportName | identifier) with? Semicolon
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
    : World identifier OpenCurly worldDefinition* CloseCurly
    ;

typeAlias
    : Type identifier Equal type Semicolon
    ;

worldDefinition
    : export
    | import_
    | include
    ;

interface
    : Interface identifier? OpenCurly interfaceDefinition* CloseCurly
    ;

interfaceDefinition
    : identifier Colon type Semicolon
    | record
    | enum
    | flags
    | typeAlias
    ;

package
    : Package packageName OpenCurly packageDefinition* CloseCurly
    ;

packageDefinition
    : world
    | interface
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
    ;

packageNamespace
    : identifier Colon (identifier Colon)*
    ;

packageName
    : packageNamespace? identifier (Slash identifier)* (At semVersion)?
    ;