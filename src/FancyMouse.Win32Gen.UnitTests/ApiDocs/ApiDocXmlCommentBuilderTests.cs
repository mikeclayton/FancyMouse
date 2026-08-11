using FancyMouse.Win32Gen.ApiDocs;

namespace FancyMouse.Win32Gen.UnitTests.ApiDocs;

public static class ApiDocXmlCommentBuilderTests
{
    // one [TestMethod] per case rather than [DynamicData], since ApiDetails
    // is internal and MSTest requires [TestMethod] parameters to be public.
    [TestClass]
    public sealed class BuildTests
    {
        [TestMethod]
        public void DescriptionOnlyProducesASingleInlineSummary()
        {
            var actual = ApiDocXmlCommentBuilder.Build(new ApiDetails
            {
                Description = "Retrieves the cursor's position.",
            });

            Assert.AreEqual("/// <summary>Retrieves the cursor's position.</summary>", actual);
        }

        [TestMethod]
        public void MultiParagraphDescriptionCollapsesToOneFlowingLine()
        {
            var actual = ApiDocXmlCommentBuilder.Build(new ApiDetails
            {
                Description = "Retrieves the cursor's\nposition.\n\nIn screen coordinates.",
            });

            Assert.AreEqual("/// <summary>Retrieves the cursor's position. In screen coordinates.</summary>", actual);
        }

        [TestMethod]
        public void ReturnsOnlyProducesAReturnsBlockWithNoReadMoreCompanion()
        {
            var actual = ApiDocXmlCommentBuilder.Build(new ApiDetails
            {
                ReturnValue = "Nonzero if successful.",
            });

            var expected = string.Join(
                "\n",
                "/// <returns>",
                "/// <para>Nonzero if successful.</para>",
                "/// </returns>");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParameterWithNoHelpLinkHasNoReadMoreCompanion()
        {
            var actual = ApiDocXmlCommentBuilder.Build(new ApiDetails
            {
                Parameters = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["hWnd"] = "A handle to the window.",
                },
            });

            var expected = string.Join(
                "\n",
                "/// <param name=\"hWnd\">",
                "/// <para>A handle to the window.</para>",
                "/// </param>");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParameterWithAHelpLinkAddsAReadMoreCompanionLinkingToParametersAnchor()
        {
            // note: a HelpLink alone (with no Remarks text) also triggers a
            // trailing <remarks> block of its own - see
            // HelpLinkWithNoRemarksTextStillEmitsARemarksBlock.
            var actual = ApiDocXmlCommentBuilder.Build(new ApiDetails
            {
                HelpLink = new Uri("https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getcursorpos"),
                Parameters = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["lpPoint"] = "A pointer to a POINT structure.",
                },
            });

            var expected = string.Join(
                "\n",
                "/// <param name=\"lpPoint\">",
                "/// <para>A pointer to a POINT structure.</para>",
                "/// <para><see href=\"https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getcursorpos#parameters\">Read more on learn.microsoft.com</see>.</para>",
                "/// </param>",
                "/// <remarks>",
                "/// <para><see href=\"https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getcursorpos#\">Read more on learn.microsoft.com</see>.</para>",
                "/// </remarks>");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemarksWithAHelpLinkAddsAReadMoreCompanionLinkingToTheBareAnchor()
        {
            var actual = ApiDocXmlCommentBuilder.Build(new ApiDetails
            {
                HelpLink = new Uri("https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getcursorpos"),
                Remarks = "The cursor position is always in screen coordinates.",
            });

            var expected = string.Join(
                "\n",
                "/// <remarks>",
                "/// <para>The cursor position is always in screen coordinates.</para>",
                "/// <para><see href=\"https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getcursorpos#\">Read more on learn.microsoft.com</see>.</para>",
                "/// </remarks>");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HelpLinkWithNoRemarksTextStillEmitsARemarksBlock()
        {
            var actual = ApiDocXmlCommentBuilder.Build(new ApiDetails
            {
                HelpLink = new Uri("https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getcursorpos"),
            });

            var expected = string.Join(
                "\n",
                "/// <remarks>",
                "/// <para><see href=\"https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getcursorpos#\">Read more on learn.microsoft.com</see>.</para>",
                "/// </remarks>");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EmptyDetailsProduceEmptyOutput()
        {
            var actual = ApiDocXmlCommentBuilder.Build(new ApiDetails());

            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        public void EverySectionCombinesInSummaryParamReturnsRemarksOrder()
        {
            var actual = ApiDocXmlCommentBuilder.Build(new ApiDetails
            {
                Description = "Retrieves the cursor's position.",
                HelpLink = new Uri("https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getcursorpos"),
                Parameters = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["lpPoint"] = "A pointer to a POINT structure.",
                },
                ReturnValue = "Returns nonzero if successful.",
                Remarks = "The cursor position is always in screen coordinates.",
            });

            var expected = string.Join(
                "\n",
                "/// <summary>Retrieves the cursor's position.</summary>",
                "/// <param name=\"lpPoint\">",
                "/// <para>A pointer to a POINT structure.</para>",
                "/// <para><see href=\"https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getcursorpos#parameters\">Read more on learn.microsoft.com</see>.</para>",
                "/// </param>",
                "/// <returns>",
                "/// <para>Returns nonzero if successful.</para>",
                "/// </returns>",
                "/// <remarks>",
                "/// <para>The cursor position is always in screen coordinates.</para>",
                "/// <para><see href=\"https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getcursorpos#\">Read more on learn.microsoft.com</see>.</para>",
                "/// </remarks>");
            Assert.AreEqual(expected, actual);
        }
    }
}
