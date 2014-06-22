var* emptyVar()
{
	var* v = malloc(sizeof(var));
	v->type = STRING;
	v->value.stringV = malloc(sizeof(var));
	*v->value.stringV ="\0";
}

var* assumeVar()
{
	//TODO
}

var* varCopy(var* toCopy)
{
	var* v = malloc(sizeof(var));
	v->type = toCopy->type;
	v->value = topCopy->value;
}

int varTest(var v)
{
	//TODO
}