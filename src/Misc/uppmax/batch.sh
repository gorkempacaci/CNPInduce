#! /bin/bash -l 
# 
#SBATCH -p node -n 1
#SBATCH -t 1:00

# cd $HOME
benchit/Benchit "benchit/benchmarks_quick.json" [1,2,3,4,5,6,7,8] 3 > "benchmarks_quick.txt"
# ./benchit/Benchit "benchmarks_small.json" [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16] 5 > $HOME/"benchmarks_small.txt"
# ./benchit/Benchit "benchmarks_large.json" [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16] 5 > $HOME/"benchmarks_large.txt"
