﻿using System.Linq;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class CodeGenerationTests
    {
        [Test]
        public void Should_properly_print_registration_info()
        {
            var info = AttributedModel.GetExportedRegistrations(typeof(PrintToCodeExample)).Single();

            var code = info.ToCode();
            var codeValue = code.ToString();
            Assert.That(codeValue, Is.EqualTo(@"
    new ExportedRegistrationInfo {
        ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample),
        Exports = new[] {
            new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample), 1, DryIoc.IfAlreadyRegistered.AppendNotKeyed),
            new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IPrintToCode), 1, DryIoc.IfAlreadyRegistered.AppendNotKeyed),
            },
        Reuse = DryIocAttributes.ReuseType.Singleton,
        ReuseName = null,
        OpenResolutionScope = false,
        AsResolutionCall = false,
        AsResolutionRoot = false,
        PreventDisposal = false,
        WeaklyReferenced = false,
        AllowDisposableTransient = false,
        TrackDisposableTransient = false,
        UseParentReuse = false,
        HasMetadataAttribute = false,
        FactoryType = DryIoc.FactoryType.Service,
        ConditionType = null
    }"));
        }
    }
}
