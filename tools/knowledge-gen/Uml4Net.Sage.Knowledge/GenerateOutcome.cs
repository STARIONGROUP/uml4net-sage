// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge
{
    /// <summary>
    /// The result of a <see cref="KnowledgeGenerationService.GenerateAsync"/> call.
    /// </summary>
    /// <param name="SpecExtraction">The outcome of extracting the UML specification's clause text.</param>
    /// <param name="XmiSpecExtraction">The outcome of extracting the companion XMI specification's clause text.</param>
    public sealed record GenerateOutcome(SpecExtractionOutcome SpecExtraction, SpecExtractionOutcome XmiSpecExtraction);
}
