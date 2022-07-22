using Autofac;

namespace Relativity.Sync
{
    /// <summary>
    ///     Registers dependencies in an Autofac <see cref="ContainerBuilder"/>.
    /// </summary>
    public interface IInstaller
    {
        /// <summary>
        ///     Register dependencies with the given container builder.
        /// </summary>
        /// <param name="builder">Builder for Autofac container</param>
        void Install(ContainerBuilder builder);
    }
}
