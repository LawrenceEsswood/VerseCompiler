typedef enum {BOOL, STRING, FLOAT, INT} type_t;

typedef union value_t
{
	int intV;
	float floatV;
	char* stringV;
} value_t;

typedef var
{
	type_t type;
	value_t value;
}

var* emptyVar();
var* assumeVar(char* val);
var* varCopy(var* toCopy);
int varTest(var v);