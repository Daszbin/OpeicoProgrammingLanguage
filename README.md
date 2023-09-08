# Opeico Programming Language
Opeico: A versatile programming language merging C#, Python, and R. Clean syntax, math integration, and simple code organization make it ideal for data analysis, math computations, and general programming. Exception handling and namespace import simplify development. Join us to streamline your coding experience!

# Opeico Programming Language

![Opeico Logo](Opeico.png)

Welcome to the official repository of Opeico, a versatile programming language designed to simplify coding tasks with a clean and intuitive syntax.

## Key Features

- **Clean and Readable Syntax:** Opeico's syntax is easy to understand, making it accessible for developers of all levels.

- **Math Integration:** Incorporates mathematical capabilities inspired by R and Python for data analysis and scientific computing.

- **Code Organization:** Define classes, methods, and namespaces with ease using a simplified structure.

- **Exception Handling:** Easily manage errors with built-in try-catch blocks using "$t_" and "$c_".

- **Namespace Import:** Seamlessly integrate external libraries and namespaces with "IMP: N_:".

## Getting Started

To start using Opeico, follow these steps:

1. Clone this repository.

2. Explore the `docs` folder for comprehensive documentation and tutorials.

3. Join our community by contributing to the development of Opeico. Check out our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Import External Libraries

- **Usage**: `IMP: N_: System`, `IMP: N_: System.IO`, etc.
- **Description**: Import external namespaces and libraries into your Opeico code, extending its functionality with predefined classes and functions.

### Define Classes and Methods

- **Usage**: `DOMAIN MainClass`, `DOMAIN MyMethod()`, etc.
- **Description**: Define classes and methods as fundamental building blocks for code structure, encapsulating data and behavior.

### String Manipulation

- **Usage**: `?{"Hello, World!"}@`, `?{"Result: " + result}@".
- **Description**: Easily work with text and data by using double-quoted strings and concatenating variables and expressions.

### Exception Handling

- **Usage**: `$t_ { ... } $c_ { ... }`.
- **Description**: Implement robust error management with "Try" and "Catch" blocks, ensuring your code handles exceptions gracefully.

### Math Integration

- **Usage**: `result Kind 10 / 2`, `sum Kind a Kind b`, etc.
- **Description**: Perform mathematical operations with ease, inspired by languages like R and Python, using standard arithmetic operators.


## Examples

```pubc
PubC MainClass
# Import necessary namespaces
IMP: N_: System
IMP: N_: System.IO
IMP: N_: System.Collections.Generic

    # Public method
    DOMAIN Main(
        ?{"Opeico Code Example"}@

        # Using Kind for equality comparison
        a Kind 10
        b Kind 20
        isEqual Kind (a Kind b)
        ?{isEqual}@
    )
