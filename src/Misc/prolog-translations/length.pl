% length
% {as:in, b:out}
% {{as:[6,9], b:2}, {as:[8], b:1}}
% NOT {{as:[2,1], b:1}}
% proj(and(const(b0, 0), foldl(proj(and(id, increment), {a->a, n->b, s->ab}))), {as->as, b->b})

% operators are indexed in the left-to-right order they appear in the CNP code. For example, The 'and' inside the foldl is and_2.

id(A, A). % (library)
increment(N, S) :- S is N + 1. % (library)
and_2(A, B, N, S) :- id(A, B), increment(N, S).
proj_2(A, B, AB) :- and_2(A, _, B, AB).
foldl_(B0, [], B0).
foldl_(B0, [A|As], B) :- proj_2(A, B0, Bi), foldl_(Bi, As, B).
and_1(B0, As, B) :- B0=0, foldl_(B0, As, B).
length_my(As, B) :- and_1(_, As, B).

% ?- length_my([1,2,3,4], X).
% X = 4 .

% ?- length_my([], X).
% X = 0 .