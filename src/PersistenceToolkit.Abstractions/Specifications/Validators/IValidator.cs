﻿namespace PersistenceToolkit.Abstractions.Specifications;

public interface IValidator
{
    bool IsValid<T>(T entity, ISpecification<T> specification);
}
