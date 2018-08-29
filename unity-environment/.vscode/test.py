import numpy as np

A = np.array([
    [1, 0, 0],
    [0, 1, 0]
])

b = np.array([2,2])
c = np.dot(A, b)
print(c)