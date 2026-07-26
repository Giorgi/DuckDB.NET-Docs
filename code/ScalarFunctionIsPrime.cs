connection.RegisterScalarFunction<int, bool>("is_prime", IsPrime);

var primes = connection.Query<int>("SELECT i FROM range(2, 100) t(i) WHERE is_prime(i::INT)").ToList();

//primes will be 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97
