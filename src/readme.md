# TCNP

CNP synthesizer implemented in C#.
- Object-level mutable unification for efficiency
- Paralellizable on a single machine with multi-threading
- Types for predicates (not implemented yet besides well-modedness)

## Definitions

Program definitions go here

### Program tree

```
GroundName = Name string
Name = GroundName | FreeName
GroundTerm = Constant integer | Constant string
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
  | foldl(Expression, Expression) // recursive, base
  | foldr(Expression,Expression) // recursive, base
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

