# CNPInduce 

CNPInduce is a program synthesizer that synthesizes CNP programs (see description below). CNP is a language equivalent to definite clauses (like Prolog), but CNPInduce as a methodology can be extended to any language with well-defined semantics. The efficiency of CNPInduce depends on the language, specifically by how exploitable the reversed semantics are. The more higher-level recursive operators language has, the better. Some candidates for this are SQL (aggregates like MAX, AVG, etc), C# (Query Operators like WHERE, SELECT, ORDERBY), or Spreadsheet Formulas (like COUNTIF)

For more information on CNPInduce and its applications:
- Paper that defines CNP language and CNPInduce's general algorithm: [Compositional Relational Programming with Name Projection and Compositional Synthesis](http://uu.diva-portal.org/smash/record.jsf?pid=diva2%3A1168847&dswid=855)
- Application of CNPInduce to ExplainableAI: ["Why did you do that?": Explaining black box models with inductive synthesis](https://arxiv.org/abs/1904.09273)
- PhD Thesis that describes the synthesis and the accompanying proofs of CNP being equivalent to definite clauses: [Representations of Compositional Relational Programs](https://uu.diva-portal.org/smash/record.jsf?pid=diva2%3A1080366&dswid=2983)

# How to run the benchmarks

Load the solution and run the Benchit project. Tested on .Net 7. 

`> ./Benchit benchmarks.json "[1,5]" 5`

Runs the suite in `benchmarks.json` with 1 and 5 threads, each with 5 repeats, reporting the average of all repeats with standard deviation. Ideal number of threads may depend on the machine.

Benhmark results on a Macbook Pro M3 Max 36GB:

| Name       | AST Depth | Complexity | Ex+ | Ex- | Single-threaded | Multi-threaded | Speedup | 
| ---------- | --------- | ---------- | --- | --- | --------------- | -------------- | ------- | 
| head       |         2 |       O(1) |   2 |   0 |    0.001 ±0.000 |   0.001 ±0.000 |     0.8 |
| decrement  |         3 |       O(1) |   2 |   0 |    0.021 ±0.001 |   0.007 ±0.001 |     2.8 |
| append     |         3 |       O(n) |   2 |   0 |    0.015 ±0.000 |   0.004 ±0.000 |     3.7 |
| reverse    |         4 |       O(n) |   2 |   0 |    0.184 ±0.035 |   0.027 ±0.002 |     6.9 |
| sum        |         4 |       O(n) |   2 |   0 |    0.117 ±0.000 |   0.019 ±0.001 |     6.3 |
| maxlist    |         4 |       O(n) |   2 |   0 |    0.123 ±0.000 |   0.019 ±0.001 |     6.4 |
| length     |         6 |       O(n) |   2 |   1 |    7.696 ±0.013 |   2.282 ±0.213 |     3.4 |
| flatten    |         6 |     O(n^2) |   2 |   0 |    4.614 ±0.054 |   1.538 ±0.115 |     3.0 |
| sumall     |         6 |     O(n^2) |   2 |   0 |    4.026 ±0.057 |   1.347 ±0.195 |     3.0 |

Some environment variables have to be set for multi-threading to be enabled for .Net.

On Windows:
```
  set DOTNET_GCCpuGroup=1
  set DOTNET_gcConcurrent=1
  set DOTNET_Thread_UseAllCpuGroups=1
```
On MacOS:
```
  export DOTNET_GCCpuGroup=1
  export DOTNET_gcConcurrent=1
  export DOTNET_Thread_UseAllCpuGroups=1
```
For program definitions see Example Programs section.

# Disclaimer

**Parallel CNPInduce** is under active development, so there may be times the main branch is not healthy. Check under 'Actions' that the version you're checking out is 'green', meaning all the tests have passed for that version. 

# Known bugs
- After the and-valence work, the multithreading speedup reduced from 4x to 2x. There's no blocking happening, and it doesn't help to add more threads, so it may be due to cache misses.

# Backlog
- In order to eliminate symmetry (for example happens for `len` with and(increment, increment) a post-synthesis symmetry check had to be added. This would be better handled by introducing constraints on program variables (observations). There's already a preliminary implementation of this for reducing the double applications of `proj`, which is rather ugly currently.
- Integrate the type lookup for all operands and primitives, so that it's a single hashmap lookup instead of one lookup for each.
- NameVar constraint check isn't efficient. 

# Program Examples

## head 
CNP:
`proj(cons, {ab->list, a->h})`

Prolog equivalent:
```
cons(A, B, [A|B]). % elementary predicate
head(List, H) :- cons(H, _, List).
```

## decrement
CNP:
`proj(and(const(b, 1), -), {a->n, ab->s})`

Prolog equivalent:
```
and(A, B, AB) :- B=1, AB is A - B.
decrement(N, P) :- and(N, _, P).
```

## append 
CNP:
`proj(foldr(cons), {as->list1, b0->list2, b->list3})`

Prolog equivalent:
```
cons(A, B, [A|B]). % elementary predicate
foldr_(B0, [], B0).
foldr_(B0, [A|As], B) :- foldr_(B0, As, Bi), cons(A, Bi, B).
append_my(List1, List2, List3) :-  foldr_(List2, List1, List3).
```

## reverse
CNP:
`proj(and(const(b0, []), foldl(cons)), {as->as, b->bs})`

Prolog equivalent:
```
cons(A, B, [A|B]). % elementary predicate
foldl_(B0, [], B0).
foldl_(B0, [A|As], B) :-
  cons(A, B0, Bi),
  foldl_(Bi, As, B).
and_(B0, As, B) :-
  B0=[],
  foldl_(B0, As, B).
reverse_my(As, Bs) :- 
  and_(_, As, Bs).
```

## sum
CNP:
`proj(and(const(b0, 0), foldl(+)), {as->list, b->sum})`

Prolog equivalent:
```
+(A, B, AB) :- AB is A + B. % elementary predicate
foldl_(B0, [], B0).
foldl_(B0, [A|As], B) :-
  +(A, B0, Bi),
  foldl_(Bi, As, B).
and_(B0, As, B) :-
  B0=0,
  foldl_(B0, As, B).
sum(As, Bs) :- 
  and_(_, As, Bs).
```

## maxlist 
CNP:
`proj(and(const(b0, 0), foldl(max)), {as->list, b->max})`

Prolog equivalent:
```
max(A, B, AB) :- AB is max(A, B). % elementary predicate
foldl_(B0, [], B0).
foldl_(B0, [A|As], B) :-
  max(A, B0, Bi),
  foldl_(Bi, As, B).
and_(B0, As, B) :-
  B0=0,
  foldl_(B0, As, B).
maxlist(As, Bs) :- 
  and_(_, As, Bs).
```

## length 
CNP:
`proj(and(const(b0, 0), foldl(proj(and(id, increment), {a->a, n->b, s->ab}))), {as->as, b->b})`

Prolog equivalent:
```
id(A, B) :- A=B. % elementary predicate
increment(N, S) :- S is N+1. % elementary predicate
and_2(A, B, N, S) :- id(A, B), increment(N, S).
proj_2(A, B, AB) :- and_2(A, _, B, AB).
foldl_(B0, [], B0).
foldl_(B0, [A|As], B) :- proj_2(A, B0, Bi), foldl_(Bi, As, B).
and_1(B0, As, B) :- B0=0, foldl_(B0, As, B).
length_my(As, B) :- and_1(_, As, B).
```

## flatten 
CNP:
`proj(and(const(b0, []), foldr(proj(foldr(cons), {as->a, b0->b, b->ab}))), {as->as, b->bs})`

Prolog equivalent:
```
cons(A, B, [A|B]). % elementary predicate
foldr_2(B0, [], B0).
foldr_2(B0, [A|As], B) :- foldr_2(B0, As, Bi), cons(A, Bi, B).
proj_2(A, B, AB) :- foldr_2(B, A, AB).
foldr_1(B0, [], B0).
foldr_1(B0, [A|As], B) :- foldr_1(B0, As, Bi), proj_2(A, Bi, B).
and_(B0, As, B) :- B0=[], foldr_1(B0, As, B).
flatten_my(As, Bs) :- and_(_, As, Bs).
```

## sumall
CNP:
`proj(and(const(b0, 0), foldl(proj(foldl(+), {as->a, b0->b, b->ab}))), {as->lists, b->sum})`

Prolog equivalent:
```
+(A, B, AB) :- AB is A+B. % elementary predicate
foldr_2(B0, [], B0).
foldr_2(B0, [A|As], B) :- foldr_2(B0, As, Bi), +(A, Bi, B).
proj_2(A, B, AB) :- foldr_2(B, A, AB).
foldr_1(B0, [], B0).
foldr_1(B0, [A|As], B) :- foldr_1(B0, As, Bi), proj_2(A, Bi, B).
and_(B0, As, B) :- B0=0, foldr_1(B0, As, B).
sumall_my(As, Bs) :- and_(_, As, Bs).
```

# Parallel CNPInduce
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

The language is similar to Relational Algebra, where the construction of expressions is compositional, much like functional languages, but any constructed expression is relation-valued: it stands for a relation, that is, a set of tuples. This relation is called the 'relational extension' of the constructed expression. Extensions of the terminal constructs "elementary predicates" id, cons, and const are ground, while the extensions of the operators are given in relation to the extensions of their operands.
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

### A simple math library

The following math functions and predicates are available to the synthesis:

`lt {a:A, b:B} :- A < B`

`lte {a:A, b:B} :- A <= B`

`plus {a:A, b:B, ab:AB} :- AB is A+B`

`mul {a:A, b:B, ab:AB} :- AB is A*B`

`min {a:A, b:B, ab:AB} :- AB is Math.Min(A,B)`

`max {a:A, b:B, ab:AB} :- AB is Math.Max(A,B)`

`increment {n:N, s:S} :- S is N + 1`

Adding more is straightforward by changing `MathLib.cs`.

## Searchable-CNP, Searchable language

Searchable-CNP, as CNP implemented in the accompanying code, is an extended version of CNP to carry along search parameters in the syntax tree. 

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

### Other definitions and implementation notes

#### Open/Closed expressions/programs

A program is closed if it doesn't contain any observations.

#### Ground/Unground terms and names

A Name is ground if it is not a Free name. A valence is ground if all its names are ground. A program is ground if it does not contain any observations, and all its names appearing in projections and consts are ground.

Note: Observations may contain Free terms, which would make them not ground, but this is not of concern since if a program does contain observations, it's not closed, and won't be considered as a correct solution anyway. 

### Implementing CreateAtFirstHole

- The names in an observation may not be ground, so trying to access the terms of alphatuples by name without checking that the names are ground is an error and will lead to an exception. The way to approach this is to ground t
 possibly by using the PossibleGroundings method on the non-ground valence, and calling this method against a ground valence.

- Returned list of programs may be open and unground.

### Reversing argument names for logic operators

(i,m) \in V = domains for the and/or expression for 1<=i<=6
c \in C = common domains to Vp and Vq, found by all unordered combinations of (n,_) \in V




