﻿namespace PersistenceToolkit.Abstractions.Specifications;

public class InMemorySpecificationEvaluator : IInMemorySpecificationEvaluator
{
    // Will use singleton for default configuration. Yet, it can be instantiated if necessary, with default or provided evaluators.
    public static InMemorySpecificationEvaluator Default { get; } = new InMemorySpecificationEvaluator();

    protected List<IInMemoryEvaluator> Evaluators { get; }

    public InMemorySpecificationEvaluator()
    {
        Evaluators =
        [
            WhereEvaluator.Instance,
            SearchMemoryEvaluator.Instance,
            OrderEvaluator.Instance,
            PaginationEvaluator.Instance
        ];
    }

    public InMemorySpecificationEvaluator(IEnumerable<IInMemoryEvaluator> evaluators)
    {
        Evaluators = evaluators.ToList();
    }

    public virtual IEnumerable<TResult> Evaluate<T, TResult>(IEnumerable<T> source, ISpecification<T, TResult> specification)
    {
        if (specification.Selector is null && specification.SelectorMany is null) throw new SelectorNotFoundException();
        if (specification.Selector != null && specification.SelectorMany != null) throw new ConcurrentSelectorsException();

        var baseQuery = Evaluate(source, (ISpecification<T>)specification);

        var resultQuery = specification.Selector != null
          ? baseQuery.Select(specification.Selector.Compile())
          : baseQuery.SelectMany(specification.SelectorMany!.Compile());

        return specification.PostProcessingAction is null
            ? resultQuery
            : specification.PostProcessingAction(resultQuery);
    }

    public virtual IEnumerable<T> Evaluate<T>(IEnumerable<T> source, ISpecification<T> specification)
    {
        foreach (var evaluator in Evaluators)
        {
            source = evaluator.Evaluate(source, specification);
        }

        return specification.PostProcessingAction is null
            ? source
            : specification.PostProcessingAction(source);
    }
}
