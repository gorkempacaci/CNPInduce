== Haskell Folds ==

foldr :: (a->b->b) -> b -> ([a] -> b)
foldr f z []     = z 
foldr f z (x:xs) = f x (foldr f z xs) 

// foldl :: (b->a->b) -> b -> ([a] -> b)
foldl :: (a->b->a) -> a -> ([b] -> a)
foldl f z []     = z                  
foldl f z (x:xs) = foldl f (f z x) xs

== Prolog folds ==

foldl(:Goal, +List, +V0, -V).
foldl(G, [X_11, ..., X_1n],
         [X_21, ..., X_2n],
         ...,
         [X_m1, ..., X_mn], V0, V) :-
  call(G, X_11, ..., X_m1, V0, V),
  call(G, X_12, ..., X_m2, V1, V2),
  ...
  call(G, X_1n, ..., X_,m, V<n-1>, V).

?- foldl(plus, [1,2,3], 0, N).
N = 6.

== Combilog folds ==

foldr(P, Q)(Y, [], Z) :- Q(Y, Z).
foldr(P, Q)(Y, [X|T], W) :- foldr(P, Q)(Y, T, Z), P(X, Z, W).

foldl(P, Q)(Y, [], Z) :- Q(Y, Z).
foldl(P, Q)(Y, [X|T], Q) :- P(X, Y, Z), foldl(P, Q)(Z, T, W).

== CNP folds ==

foldr :: {a:A, b:B, ab:B} -> {b0:B, as:[A], b:B}
foldr(P) {b0:B0, as:[], b:B0}.
foldr(P) {b0:B0, as:[A|As], b:B} :- foldr(P){b0:B0, as:As, b:Bi} ^ P{a:A, b:Bi, ab:B}.

foldl :: {a:A, b:B, ab:B} -> {b0:B, as:[A], b:B}
foldl(P){b0:B0, as:[], b:B0}.
foldl(P){b0:B0, as:[A|As], b:B} :- P{a:A, b:B0, ab:Bi} ^ foldl(P){b0:Bi, as:As, b:B}.

=== examples ===

foldr(cons){b:[], as:[1,2,3], b:B} :-
    foldr(cons){b0:[], as:[], b:B3} ^ cons{a:3, b:B3, ab:B2} ^ cons{a:2, b:B2, ab:B1} ^ cons{a:1, b:B1, ab:B}.
                                []                []     [3]               [3]    [2,3]             [2,3]  [1,2,3]

foldl(cons){b:[], as:[1,2,3], b:B} :-
    cons{a:1, b:[], ab:B1} ^ cons{a:2, b:B1, ab:B2} ^ cons{a:3, b:B2, ab:B3}, foldl(cons){b0:B3, as:[], b:B}.
                       [1]               [1]    [2,1]             [2,1]  [3,2,1]             [3,2,1]      [3,2,1]

== Other definitions ==

Haskell  map :: (a -> b) -> ([a] -> [b])
         map f = fold(λx xs. f x : xs) []

map :: {a:A, b:B} -> {as:[A], bs:[B]}
map(P){as:[], bs:[]}.
map(P){as:[A|As], bs:Bs} :- map(P){as:As, bs:Bt} ^ P{a:A, b:B} ^ cons{a:B, b:Bt, ab:Bs}.

map(abs){as:[1,-3,4], bs:Bs}
    :- map(abs){as:[], bs:Bs3} ^ abs{a:4,  b:B3} ^ cons{B3, Bs3,  Bs2}
                          []                 4          4   []    [4]
                               ^ abs{a:-3, b:B2} ^ cons{B2, Bs2,  Bs1}
                                             3          3   [4]   [3,4] 
                               ^ abs{a:1,  b:B1} ^ cons{B1, Bs1,  Bs}.
                                             1          1   [3,4] [1,3,4]

    
-4, [] -> [4]
-3, [4] -> [3,4]
-1, [3,4] -> [1,3,4]

abs' = proj(and(abs, proj(cons, {a->b, b->bi, ab->ab}), {a->a, bi->b, ab->ab})

map(P) = foldr(proj(and(P, proj(cons, {a->b, b->bi, ab->ab}), {a->a, bi->b, ab->ab})) []
   (no currying so [] needs to be and(const[], ...).

Define currying as a higher order op?