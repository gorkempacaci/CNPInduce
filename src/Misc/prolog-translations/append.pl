% append
% {list1:in, list2:in, list3:out}
% proj(foldr(cons), {as->list1, b0->list2, b->list3})
% {{list1:[], list2:[1], list3:[1]}, {list1:[1,2], list2:[3,4], list3:[1,2,3,4]}}

cons(A, B, [A|B]).  % (library)
foldr_(B0, [], B0).
foldr_(B0, [A|As], B) :-
  foldr_(B0, As, Bi),
  cons(A, Bi, B).
append_my(List1, List2, List3) :- 
  foldr_(List2, List1, List3).

% ?- append_my([1,2,3], [4,5,6], X).
% X = [1, 2, 3, 4, 5, 6] .
