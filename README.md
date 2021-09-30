# TCNP

CNP synthesizer implemented in C#.
- Object-level mutable unification for efficiency
- Paralellizable on a single machine with multi-threading
- Types for predicates (not implemented yet besides well-modedness)

## CNP Language ##

CNP language is defined as follows, where P is a program in CNP, N is an identifier name, C is a constant term (i.e. 1, 4.5, 'hello'). A separate syntax definition follows in the next section specifially for search.

P = id
  | cons
  | const(N, T)
  | and(P, P)
  | or(P, P)
  | foldr(P, P)
  | foldr(P)
  | foldl(P, P)
  | foldl(P)
  | map(P)
  | proj(P, M)

The language is similar to Relational Algebra, where the construction of expressions is compositional, much like functional languages, but any constructed expression is relation-valued: it stands for a relation, that is, a set of tuples. This relation is called the 'relational extension' of the constructed expression. Extensions of the elementary constructs id, cons, and const are ground and terminal, while the extensions of the operators are given in relation to the extensions of their operands.
Predicate Expressions are built from these constructs, and a Predicate Expression is said to be true, or that it 'succeeds' with regard to a given tuple, if the tuple exists in its relational extension. This is the concept of relational extension from Predicate Logic. CNP is a compositional interpretation of Predicate Logic, or more specifically Definite Clauses, built on earlier work called COMBILOG.

- id is a set of tuples {a:X, b:Y}, where X and Y are identical. It's the identity relation, therefore it's an infinite set. id succeeds for tuples such as {a:1, b:1}, or {a:'hello', b:'hello'}.

- cons is a set of tuples {a:X, b:Y, c:[X|Y]}, where the value for 'c' is a compound term consisting of X and Y. Likewise, this is an infinite set. cons succeeds for tuples such as {a:1, b:[2,3], c:[1,2,3]}, or {a:1, b:[], c:[1]}.

- const(N, T) is parametric, and it has a single tuple {N:T}, simply mapping the given name to the given term. For example, const('number', 5) is equivalent to the relation {{'number':5}}. Therefore const('number',5) succeeds for, and only for, the tuple {'number',5'}. Since no other construct takes a term as a parameter, this is the only way to get data embedded in CNP code. There are no IO routines, or variables either. 

The names that appear in the tuples of a relation are the 'names' of that relation, therefore the names for that expression. id has {a, b} for names, therefore we write id : {a, b} as its signature. Names are not ordered, therefore we write them as a set {}.

- and(P, Q) takes two programs as operands, and produces a predicate expression. P and Q need to have at least one common name. The names of the expression and(P, Q) are the union of names in P and Q. The tuples in the extension of and(P,Q) are determined as the intersection of those in P and Q, with regard to the common names, as in the NATURAL JOIN operator in Relational Algebra. For example, the predicate expression and(id, cons) has the names {a, b, ab}, and it  succeeds for {a:[], b:[], ab:[[]]}. This is because id succeeds for {a:[], b:[]}, and cons succeeds for {a:[], b:[], ab:[[]]}. Likewise, it succeeds for {a:[1], b:[1], ab:[[1],1]}, and {a:[1,2], b:[1,2], ab:[[1,2],1,2]}.

- or(P, Q) takes two programs as operands, and produces a predicate exression. Like and, P and Q need to have at least one common name, and the names of the expression or(P, Q) are the union of names in P and Q. Unlike and, its extension is defined as the UNION of those in P's and Q's. For example, or(id, cons) succeeds for {a:1, b:2, ab:[1,2]}, because while id does not succeed for {a:1, b:2}, cons does for {a:1, b:2, ab:[1,2]}. This operator is near-identical to the OUTER JOIN in Relational Algebra.

- foldr(P, Q) is a fixed-point operator in relation to P as the 'folded' expression, and Q as the one used to generate the initial value in the base case. The names of foldr(P, Q), P, and Q are fixed. foldr(P,Q):{b0, as, b}, P:{a, b, ab}, Q:{a, b}. For example, foldr(cons, id) gives an append predicate expression. In this case Q is given as id, which is not ver useful, but having Q as an operand is useful in some other cases where you want to transform the given seed value to start with. foldr(cons,id) succeeds for {b0:[4], as:[1,2,3], b:[1,2,3,4]}, because id succeeds for {a:[4], b:[4]}, giving [4] for the initial value, and then cons suceeds for {a:3, b:[4], ab:[3,4]}, {a:2, b:[3,4], ab:[2,3,4]}, {a:1, b:[2,3,4], ab:[1,2,3,4]}.

- foldr(P) as an overload assumes Q=id.

- foldl(P, Q) complements foldr, except in the order the elements are 'folded'. foldl(cons, id) gives a reverse predicate, and succeeds for {b0:[], as:[1,2,3], b:[3,2,1]}. id, gives [] as the initial value. cons succeeds for {a:1, b:[], b:[1]}, {a:2, b:[1], ab:[2,1]}, {a:3,b:[2,1],b:[3,2,1]}.

- foldl(P) as an overload assumes Q=id.

- map(P) takes a single P as an operand, and succeeds for a tuple {as:X, bs:Y} where X and Y are lists of the same length, and P succeeds for each pair of corresponding elements. For example, map(id) succeeds for {as:[1,2,3], bs:[1,2,3]}, because id succeeds for {a:1, b:1}, {a:2, b:2}, {a:3, b:3}.

- proj(P, M) is projection of a predicate expression P, where names are changed according to the projection map given in M. For example, proj(cons, {a:head, b:tail, ab:list}) succeeds for any list in the form {head:1, tail:[2,3], list:[1,2,3]}, because cons succeeds for {a:1, b:[2,3], ab:[1,2,3]}.

## Open-CNP, Searchable language

Open-CNP, as CNP implemented in the accompanying code, is an extended version of CNP to carry along search parameters in the syntax tree. 

### AST

```
Name = Name string | FreeName
GroundTerm = Value integer | Value string
Term = GroundTerm | Free
Tuple = set((Name, Term))
TupleSet = set(Tuple)
Mode = in | out
Valence = set((Name, Mode))
ProjectionMap = set((Name, Name)) // from name to name
Expression
  = id | cons | const(Name, Value)
  | and(Expression, Expression)
  | or(Expression, Expression)
  | foldr(Expression,Expression) // recursive, base
  | foldr(Expression)
  | foldl(Expression, Expression) // recursive, base
  | foldl(Expression)
  | map(Expression)
  | proj(Expression, ProjectionMap)
  | Observation(Valence, TupleSet) 
```

#### Open/Closed expressions/programs

A program is closed if it doesn't contain any observations.

#### Ground/Unground terms and names

A Name is ground if it is not a Free name. A valence is ground if all its names are ground. A program is ground if it does not contain any observations, and all its names appearing in projections and consts are ground.

*Note: Observations may contain Free terms, which would make them not ground, but this is not of concern since if a program does contain observations, it's not closed, and won't be considered as a correct solution anyway. *

### Implementing CreateAtFirstHole

- The names in an observation may not be ground, so trying to access the terms of alphatuples by name without checking that the names are ground is an error and will lead to an exception. The way to approach this is to ground t
 possibly by using the PossibleGroundings method on the non-ground valence, and calling this method against a ground valence.

- Returned list of programs may be open and unground.

### Reversing argument names for logic operators

(i,m) \in V = domains for the and/or expression for 1<=i<=6
c \in C = common domains to Vp and Vq, found by all unordered combinations of (n,_) \in V




