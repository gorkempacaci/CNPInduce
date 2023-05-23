cons(H, T, [H|T]).

% reverse
p(As, Bs) :- p1(As, [], Bs).
p1([], B0, B) :- B0=B.
p1(As, B0, B) :-
  cons(A, Tail, As),
  p1(Tail,[A|B0],B).
