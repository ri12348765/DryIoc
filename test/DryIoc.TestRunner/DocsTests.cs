namespace DryIoc.Docs;

public class CreatingAndDisposingContainer : ITest
{
    public int Run()
    {
        new Creating_container_with_explicit_defaults().Example();
        new Adding_some_rules().Example();
        new Adding_some_rules_with_action().Example();
        new Disposing_container().Example();
        return 4;
    }
}

public class Decorators : ITest
{
    public int Run()
    {
        new Decorator_with_logger().Example();
        new Decorator_of_service_with_serviceKey().Example_with_condition();
        new Decorator_of_service_with_serviceKey().Example_with_DecoratorOf();
        new Decorator_of_service_with_key_and_type().Example_with_DecoratorOf_type_and_key();
        new Decorator_of_service_with_key_and_type().Example_with_DecoratorOf_type();
        new Nested_decorators().Example();
        new Nested_decorators_order().Example();
        new Open_generic_decorators().Example();
        new Decorator_of_generic_T_type().Example();
        new Decorator_of_generic_T_type_with_condition().Example();
        new Decorator_reuse().Example();
        new Decoratee_reuse().Example();
        new Decorator_of_wrapped_service().Example();
        new Nesting_decorators_of_wrapped_service().Example();
        new Collection_wrapper_of_non_keyed_and_keyed_services().Example();
        new Decorator_of_wrapper().Example();
        new Decorator_as_initializer().Example();
        new Reusing_the_scoped_service_from_the_parent_scope().Example();
        new Using_the_Decorator_directly_for_the_complex_initialization().Example();
        return 19;
    }
}

public class ErrorDetectionAndResolution : ITest
{
    public int Run()
    {
        new Unable_to_resolve_unknown_service().Example();
        new Unable_to_resolve_from_registered_services().Example();
        new No_current_scope_available().Example();
        new Recursive_dependency_detected().Example();
        new Allow_a_recursive_dependencies().Example();
        new Allow_recursive_dependency_in_DryIoc().Example();
        new Registrations_diagnostics().Example();
        new Validate_CaptiveDependency_example().Scoped_in_a_Singleton_should_be_reported_by_Validate();
        return 8;
    }
}

public class ExamplesContextBasedResolution : ITest
{
    public int Run()
    {
        new Log4net_logger_example().Example();
        new Serilog_logger_example().Example();
        return 2;
    }
}

public class Interception : ITest
{
    public int Run()
    {
        new Register_and_use_interceptor().Example();
        new Register_and_use_async_interceptor().Example().GetAwaiter().GetResult();
        new Register_and_use_interceptor_with_LinFu().Example();
        return 3;
    }
}