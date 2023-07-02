% flatten
% {as:in, bs:out}
% {{as:[[1,2], [], [3]], bs:[1,2,3]}, {as:[[9]], bs:[9]}}
% proj(and(const(b0, []), foldr(proj(foldr(cons), {as->a, b0->b, b->ab}))), {as->as, b->bs})

% cnp(fold(_), _{b0:B0, as:[], b:B0}).
% cnp(fold(P), _{b0:B0, as:[A|As], b:B}) :-
%   !,
%   cnp(fold(P), _{b0:B0, as:As, b:Bi}),
%   cnp(P, _{a:A, b:Bi, ab:B}).

cons(A, B, [A|B]). % (library)
foldr_2(B0, [], B0).
foldr_2(B0, [A|As], B) :- foldr_2(B0, As, Bi), cons(A, Bi, B).
proj_2(A, B, AB) :- foldr_2(B, A, AB).
foldr_1(B0, [], B0).
foldr_1(B0, [A|As], B) :- foldr_1(B0, As, Bi), proj_2(A, Bi, B).
and_(B0, As, B) :- B0=[], foldr_1(B0, As, B).
flatten_my(As, Bs) :- and_(_, As, Bs).

% ?- flatten_my([[1,2], [3,4], [5,6]], X).
% X = [1, 2, 3, 4, 5, 6] ;