% reverse
% {as:in, bs:out}
% {{as:['a'], bs:['a']}, {as:[1,2,3], bs:[3,2,1]}}
% proj(and(const(b0, []), foldl(cons)), {as->as, b->bs})

cons(A, B, [A|B]).  % (library)
foldl_(B0, [], B0).
foldl_(B0, [A|As], B) :-
  cons(A, B0, Bi),
  foldl_(Bi, As, B).
and_(B0, As, B) :-
  B0=[],
  foldl_(B0, As, B).
reverse_my(As, Bs) :- 
  and_(_, As, Bs).

% ?- reverse_my([1,2,3], X).
% X = [3, 2, 1] .

% ?- reverse_my([], X).
% X = [] ;