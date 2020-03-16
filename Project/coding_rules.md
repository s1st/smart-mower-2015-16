# Coding Rules

## Microsoft Coding Guidelines

C# overview - see Document or
[Online Documentation](https://msdn.microsoft.com/de-de/library/ff926074.aspx).

Line Length: 80

## Method, Property and Variable Naming

1. CamelCase is the de-facto standard in C#
2. Method names begin upper case:
```
public void MyFunc(int i) {...}
```

3. Variable names begin lower case:
```
int myInt = i;
```

4. Properties in general begin lower case. An exception to this are get- and set- properties:
```
public int myCounter;
// ... //
private int myCounter2;
public int MyCounter2
{
	get {return myCounter2;}
	set {myCounter2 = value;}
}
```

## Documentation

Documentation: Doxygen or Microsoft original syntax? See this
[Thread](http://stackoverflow.com/questions/2028264/visual-studio-with-doxygen-for-documentation-or-should-we-use-something-else).
