# Loop ↔ Copilot ↔ Repo Toolchain — Working Pattern (Battle‑Tested)

This document summarizes the **practical, copy‑paste‑safe toolchain** proven to work for Sprint 0 design artifacts (ontology, event storming, PCI controls), especially when Mermaid diagrams are involved.

---

# Exact Tool + Copy/Paste Process (Ordered)

1. **Conduct the interview in Loop**
   - Use Loop as the live collaboration space.
   - Capture ontology or event‑storming interviews in plain text.
   - Use headings, bullets, and narrative only.
   - Do *not* author Mermaid diagrams or complex Markdown here.

2. **Loop interview into Copilot via share link**
   - Ask Copilot to:
     - Clean up language
     - Normalize terminology
     - Structure the content as Markdown
     - Add Mermaid diagrams where appropriate
   - Treat Copilot as the transformation and cleanup step.

3. **Ask Copilot to output ONE single code block**
   - The code block must contain:
     - The entire Loop page content
     - All headings, text, and Mermaid blocks
   - This is critical to prevent Loop from reformatting or corrupting Markdown.

4. **Copy the entire code block from Copilot**
   - Copy everything inside the single fenced code block.
   - Do not modify it during copy.

5. **Paste the code block into Loop**
   - Paste it as-is so Loop treats it as opaque text.
   - Verify nothing was reformatted.
   - This step is only for holding and visibility, not editing.

6. **Copy from Loop into the repository**
   - Copy the full content from the code block.
   - Paste directly into a `.md` file in the repo.
   - Save and verify Markdown + Mermaid rendering in the repo.

7. **Treat the repo as the source of truth**
   - All future edits happen in the repo.
   - Loop only references the content (snippets, screenshots, or links).
   - Never re‑round‑trip Markdown from repo back into Loop for editing.

8. **Repeat the pattern for future Sprint 0 artifacts**
   - Loop = interviews and discussion
   - Copilot = structuring, diagrams, cleanup
   - Repo = canonical storage
   - Single code block = safe transport mechanism

---

## Roles and Responsibilities

### Loop (Collaboration Surface)
**Purpose**
- Stakeholder interviews
- Narrative exploration
- Early drafts and discussion
- Human‑readable context

**Rules**
- Use plain text, headings, and bullets
- Avoid authoring Mermaid or canonical Markdown here
- Treat Loop as *conversational*, not authoritative

**Strength**
- Excellent rendering (including Mermaid)
- Great for review and discussion

**Limitation**
- Not safe for Markdown round‑tripping
- Auto‑formats content unless explicitly prevented

---

### Copilot (Transformation & Cleanup)
**Purpose**
- Convert interviews → structured Markdown
- Normalize terminology
- Generate Mermaid diagrams
- Enforce ontology vs event‑storming separation
- Prepare repo‑ready artifacts

**Key Technique**
- When moving content across tools, **request a single enclosing code block**
- This forces tools (including Loop) to preserve content verbatim

---

### Repository (Source of Truth)
**Purpose**
- Canonical Markdown files
- Mermaid diagrams
- Version control and review
- CI / documentation pipelines

**Rules**
- All final artifacts live here
- Never re‑edit repo content inside Loop
- If changes are needed: edit in repo, then paste *rendered excerpts* into Loop

---

## Proven Authoring Workflow

1. **Interview in Loop**
   - Capture ontology or event‑storming discussion in plain text
   - Focus on clarity, not formatting
   - No diagrams at this stage

2. **Ask Copilot to Structure**
   - Provide interview text
   - Ask for cleaned, structured Markdown
   - Ask explicitly for:
     - Ontology section
     - Event storming section
     - Mermaid diagrams added where appropriate

3. **Transport via Single Code Block**
   - Request **one code block containing the entire page**
   - Copy from Copilot → paste into Loop (as code)
   - Copy from Loop → paste into `.md` file in repo

4. **Store Canonically in Repo**
   - Save Markdown file
   - Verify Mermaid rendering
   - Commit as source of truth

5. **Reference Back to Loop (Optional)**
   - Paste rendered snippets or links
   - Use Loop only for discussion and visibility

---

## Golden Rule

> **Loop is for discussion.  
> Git is for truth.  
> Code blocks are the transport mechanism.**

If a tool tries to *help* by reformatting your Markdown, wrap everything in one fenced block and move on.

---

## Outcome

- Reliable copy/paste
- Clean separation of concerns
- No fighting renderers
- Sprint 0 artifacts stay stable, reviewable, and teachable

This workflow is now validated and repeatable.