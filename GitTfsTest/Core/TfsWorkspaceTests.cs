using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Sep.Git.Tfs.Commands;
using Sep.Git.Tfs.Core;
using Sep.Git.Tfs.Core.TfsInterop;
using Assert = Xunit.Assert;

namespace Sep.Git.Tfs.Test.Core
{
    [TestClass]
    public class TfsWorkspaceTests
    {
        [TestMethod]
        public void Nothing_to_checkin()
        {
            IWorkspace workspace = MockRepository.GenerateStub<IWorkspace>();
            string localDirectory = string.Empty;
            TextWriter writer = new StringWriter();
            TfsChangesetInfo contextVersion = MockRepository.GenerateStub<TfsChangesetInfo>();
            IGitTfsRemote remote = MockRepository.GenerateStub<IGitTfsRemote>();
            CheckinOptions checkinOptions = new CheckinOptions();
            ITfsHelper tfsHelper = MockRepository.GenerateStub<ITfsHelper>();
            CheckinPolicyEvaluator policyEvaluator = new CheckinPolicyEvaluator();

            TfsWorkspace tfsWorkspace = new TfsWorkspace(workspace, localDirectory, writer, contextVersion, remote, checkinOptions, tfsHelper, policyEvaluator);

            workspace.Stub(w => w.GetPendingChanges()).Return(null);

            var ex = Assert.Throws<GitTfsException>(() =>
            {
                var result = tfsWorkspace.Checkin();
            });

            Assert.Equal("Nothing to checkin!", ex.Message);
        }

        [TestMethod]
        public void Checkin_failed()
        {
            IWorkspace workspace = MockRepository.GenerateStub<IWorkspace>();
            string localDirectory = string.Empty;
            TextWriter writer = new StringWriter();
            TfsChangesetInfo contextVersion = MockRepository.GenerateStub<TfsChangesetInfo>();
            IGitTfsRemote remote = MockRepository.GenerateStub<IGitTfsRemote>();
            CheckinOptions checkinOptions = new CheckinOptions();
            ITfsHelper tfsHelper = MockRepository.GenerateStub<ITfsHelper>();
            CheckinPolicyEvaluator policyEvaluator = new CheckinPolicyEvaluator();

            TfsWorkspace tfsWorkspace = new TfsWorkspace(workspace, localDirectory, writer, contextVersion, remote, checkinOptions, tfsHelper, policyEvaluator);

            IPendingChange pendingChange = MockRepository.GenerateStub<IPendingChange>();
            IPendingChange[] allPendingChanges = new IPendingChange[] { pendingChange };
            workspace.Stub(w => w.GetPendingChanges()).Return(allPendingChanges);

            ICheckinEvaluationResult checkinEvaluationResult =
                new StubbedCheckinEvaluationResult();

            workspace.Stub(w => w.EvaluateCheckin(
                                    Arg<TfsCheckinEvaluationOptions>.Is.Anything,
                                    Arg<IPendingChange[]>.Is.Anything,
                                    Arg<IPendingChange[]>.Is.Anything,
                                    Arg<string>.Is.Anything,
                                    Arg<ICheckinNote>.Is.Anything,
                                    Arg<IEnumerable<IWorkItemCheckinInfo>>.Is.Anything))
                    .Return(checkinEvaluationResult);

            workspace.Expect(w => w.Checkin(
                                    Arg<IPendingChange[]>.Is.Anything,
                                    Arg<string>.Is.Anything,
                                    Arg<ICheckinNote>.Is.Anything,
                                    Arg<IEnumerable<IWorkItemCheckinInfo>>.Is.Anything,
                                    Arg<TfsPolicyOverrideInfo>.Is.Anything,
                                    Arg<bool>.Is.Anything))
                      .Return(0);

            var ex = Assert.Throws<GitTfsException>(() =>
            {
                var result = tfsWorkspace.Checkin();
            });

            Assert.Equal("Checkin failed!", ex.Message);
        }

        [TestMethod]
        public void Policy_failed()
        {
            IWorkspace workspace = MockRepository.GenerateStub<IWorkspace>();
            string localDirectory = string.Empty;
            TextWriter writer = new StringWriter();
            TfsChangesetInfo contextVersion = MockRepository.GenerateStub<TfsChangesetInfo>();
            IGitTfsRemote remote = MockRepository.GenerateStub<IGitTfsRemote>();
            CheckinOptions checkinOptions = new CheckinOptions();
            ITfsHelper tfsHelper = MockRepository.GenerateStub<ITfsHelper>();
            CheckinPolicyEvaluator policyEvaluator = new CheckinPolicyEvaluator();

            TfsWorkspace tfsWorkspace = new TfsWorkspace(workspace, localDirectory, writer, contextVersion, remote, checkinOptions, tfsHelper, policyEvaluator);

            IPendingChange pendingChange = MockRepository.GenerateStub<IPendingChange>();
            IPendingChange[] allPendingChanges = new IPendingChange[] { pendingChange };
            workspace.Stub(w => w.GetPendingChanges()).Return(allPendingChanges);

            ICheckinEvaluationResult checkinEvaluationResult =
                new StubbedCheckinEvaluationResult()
                        .WithPoilicyFailure("No work items associated.");

            workspace.Stub(w => w.EvaluateCheckin(
                                    Arg<TfsCheckinEvaluationOptions>.Is.Anything,
                                    Arg<IPendingChange[]>.Is.Anything,
                                    Arg<IPendingChange[]>.Is.Anything,
                                    Arg<string>.Is.Anything,
                                    Arg<ICheckinNote>.Is.Anything,
                                    Arg<IEnumerable<IWorkItemCheckinInfo>>.Is.Anything))
                    .Return(checkinEvaluationResult);

            workspace.Expect(w => w.Checkin(
                                    Arg<IPendingChange[]>.Is.Anything,
                                    Arg<string>.Is.Anything,
                                    Arg<ICheckinNote>.Is.Anything,
                                    Arg<IEnumerable<IWorkItemCheckinInfo>>.Is.Anything,
                                    Arg<TfsPolicyOverrideInfo>.Is.Anything,
                                    Arg<bool>.Is.Anything))
                      .Return(0);

            var ex = Assert.Throws<GitTfsException>(() =>
            {
                var result = tfsWorkspace.Checkin();
            });

            Assert.Equal("No changes checked in.", ex.Message);
            Assert.Contains("[ERROR] Policy: No work items associated.", writer.ToString());
        }

        [TestMethod]
        public void Policy_failed_and_Force_without_an_OverrideReason()
        {
            IWorkspace workspace = MockRepository.GenerateStub<IWorkspace>();
            string localDirectory = string.Empty;
            TextWriter writer = new StringWriter();
            TfsChangesetInfo contextVersion = MockRepository.GenerateStub<TfsChangesetInfo>();
            IGitTfsRemote remote = MockRepository.GenerateStub<IGitTfsRemote>();
            CheckinOptions checkinOptions = new CheckinOptions();
            ITfsHelper tfsHelper = MockRepository.GenerateStub<ITfsHelper>();
            CheckinPolicyEvaluator policyEvaluator = new CheckinPolicyEvaluator();

            TfsWorkspace tfsWorkspace = new TfsWorkspace(workspace, localDirectory, writer, contextVersion, remote, checkinOptions, tfsHelper, policyEvaluator);

            IPendingChange pendingChange = MockRepository.GenerateStub<IPendingChange>();
            IPendingChange[] allPendingChanges = new IPendingChange[] { pendingChange };
            workspace.Stub(w => w.GetPendingChanges()).Return(allPendingChanges);

            ICheckinEvaluationResult checkinEvaluationResult =
                new StubbedCheckinEvaluationResult()
                        .WithPoilicyFailure("No work items associated.");

            checkinOptions.Force = true;

            workspace.Stub(w => w.EvaluateCheckin(
                                    Arg<TfsCheckinEvaluationOptions>.Is.Anything,
                                    Arg<IPendingChange[]>.Is.Anything,
                                    Arg<IPendingChange[]>.Is.Anything,
                                    Arg<string>.Is.Anything,
                                    Arg<ICheckinNote>.Is.Anything,
                                    Arg<IEnumerable<IWorkItemCheckinInfo>>.Is.Anything))
                    .Return(checkinEvaluationResult);

            workspace.Expect(w => w.Checkin(
                                    Arg<IPendingChange[]>.Is.Anything,
                                    Arg<string>.Is.Anything,
                                    Arg<ICheckinNote>.Is.Anything,
                                    Arg<IEnumerable<IWorkItemCheckinInfo>>.Is.Anything,
                                    Arg<TfsPolicyOverrideInfo>.Is.Anything,
                                    Arg<bool>.Is.Anything))
                      .Return(0);

            var ex = Assert.Throws<GitTfsException>(() =>
            {
                var result = tfsWorkspace.Checkin();
            });

            Assert.Equal("A reason must be supplied (-f REASON) to override the policy violations.", ex.Message);
            Assert.Contains("[ERROR] Policy: No work items associated.", writer.ToString());
        }
    }
}
