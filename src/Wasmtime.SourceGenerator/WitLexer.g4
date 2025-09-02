lexer grammar WitLexer;

// Whitespace and comments
WhiteSpaces			: [\t\u000B\u000C\u0020\u00A0]+ -> channel(HIDDEN);
LineTerminator		: [\r\n\u2028\u2029] -> channel(HIDDEN);
MultiLineComment	: '/*' .*? '*/' -> channel(HIDDEN);
SingleLineComment	: '//' ~[\r\n\u2028\u2029]* -> channel(HIDDEN);

// Identifiers and literals
String              : '"' ( ~["\\\r\n] | '\\' . )* '"';
Integer             : [0-9]+;

// Operators and punctuation
OpenParen			: '(' ;
CloseParen			: ')' ;
OpenCurly			: '{' ;
CloseCurly			: '}' ;
OpenAngle			: '<' ;
CloseAngle			: '>' ;
Semicolon			: ';' ;
Colon				: ':' ;
Comma				: ',' ;
Arrow				: '->' ;
Dot                 : '.' ;
Slash               : '/' ;
Unstable            : '@unstable' ;
Since               : '@since' ;
Deprecated          : '@deprecated';
Version             : 'version' ;
Feature             : 'feature' ;
At                  : '@' ;
Equal               : '=' ;
Dash                : '-' ;
Plus                : '+' ;
Underscore          : '_' ;

// Keywords (all reserved words)
As                  : 'as';
Async               : 'async';
Bool                : 'bool';
Borrow              : 'borrow';
Char                : 'char';
Constructor         : 'constructor';
Enum                : 'enum';
Export              : 'export';
F32                 : 'f32';
F64                 : 'f64';
Flags               : 'flags';
From                : 'from';
Func                : 'func';
Future              : 'future';
Import              : 'import';
Include             : 'include';
Interface           : 'interface';
List                : 'list';
Option              : 'option';
Own                 : 'own';
Package             : 'package';
Record              : 'record';
Resource            : 'resource';
Result              : 'result';
S8                  : 's8';
S16                 : 's16';
S32                 : 's32';
S64                 : 's64';
Static              : 'static';
Stream              : 'stream';
StringType          : 'string';
Tuple               : 'tuple';
Type                : 'type';
U8                  : 'u8';
U16                 : 'u16';
U32                 : 'u32';
U64                 : 'u64';
Use                 : 'use';
Variant             : 'variant';
With                : 'with';
World               : 'world';

// Identifiers: kebab-case, %prefix, allow keywords with %
fragment Percent: '%';
fragment KebabId: Letter (Letter | Digit | '-' | '_')*;

Identifier
    :   Percent? KebabId
    ;

fragment Symbol:
    '.' | '+' | '-' | '*' | '/' | '\\' | '^' | '~' | '=' | '<' | '>' | '!' | '?' | '@' | '#' | '$' | '%' | '&' | '|' | ':' | '\'' | '`'
;

fragment Sign: '+' | '-';
fragment Digit: [0-9];
fragment HexDigit: [0-9a-fA-F];
fragment Letter: [a-zA-Z];

fragment Name_: '$' (Letter | Digit | '_' | Symbol)+;

fragment String_:
    '"' (Char | '\n' | '\t' | '\\' | '\'' | '\\' HexDigit HexDigit | '\\u{' HexDigit+ '}')* '"'
;