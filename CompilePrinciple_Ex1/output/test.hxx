namespace A
{
	namespace innerModule
	{
		typedef struct internal
		{
			short c1 = "a";
			short i1 = 100000;
			short i2 = 10;
		}internal;
	}
	typedef struct innerStruct
	{
		int i3 = 100;
		int i4 = 100;
		innerModule::internal test;
	}innerStruct;
}

namespace B
{
	namespace middle
	{
		namespace inner
		{
			typedef struct bottom
			{
				unsigned short i7 = 10;
				unsigned short i8 = 10;
				unsigned int i10 = 100;
				long long i5 = 1000;
				long long i6 = 1000;
				unsigned int i9 = 100;
			}bottom;
		}
	}
}

typedef struct C
{
	short arr1 = "a";
	unsigned long i11 = 1000;
	unsigned long i12 = 100;
	char c0 = 'a';
	string c1 = "abc";
	bool c2 = true;
	float c3 = 10.901f;
	double c4 = 23.234d;
	long double c5 = 12.23456432235d;
	short arr[10] = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];
	C testBool1;
	A::innerModule::internal testBool2;
	short testInt = 2 + 5 * 2 / 3;
	float testFloat = 1.2 * 3.0 - 2.0 % 1.0;
	float testFloat2 = ~2.0;
}C;

