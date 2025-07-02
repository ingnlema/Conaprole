# 📝 Documentation Style Guide

## Purpose
This guide establishes consistent standards for all documentation under `/docs` to ensure readability, maintainability, and automated validation.

## Document Structure

### Standard Header Template
```markdown
# [Emoji] Document Title

## Purpose
Clear statement of what this document covers and why it exists.

## Audience
Who should read this document (developers, architects, operators, etc.).

## Prerequisites
- Required knowledge or setup
- Related documents to read first

## Content
[Main content goes here]

---

*Last verified: [Date] - Commit: [SHA]*
```

### File Naming Conventions
- Use lowercase with hyphens: `my-document.md`
- Descriptive names: `integration-testing-setup.md` not `testing.md`
- Group related files in subdirectories
- Use README.md for directory index pages

### Folder Structure
```
docs/
├── README.md                    # Main documentation index
├── overview/                    # High-level system overview
├── architecture/                # Technical architecture
├── modules/                     # Module-specific documentation
├── testing/                     # Testing guides and strategies
├── how-to/                      # Step-by-step tutorials
├── reference/                   # API reference and specifications
└── STYLE_GUIDE.md              # This document
```

## Markdown Standards

### Headers
- Use ATX-style headers (`#`, `##`, `###`)
- Include emojis for visual hierarchy: `# 🚀 Getting Started`
- Ensure proper heading hierarchy (don't skip levels)

### Code Blocks
- Always specify language: `````csharp`, `````bash`, `````yaml`
- Include context comments for complex code
- Verify all code compiles/executes against current codebase
- Use line numbers for reference when needed

### Lists
- Use `-` for unordered lists
- Use `1.` for ordered lists
- Add blank lines around lists
- Use consistent indentation (2 spaces)

### Links
- Use descriptive link text: `[integration testing guide](./testing/integration-tests.md)`
- Prefer relative links for internal documents
- Check all links are valid

### Diagrams
- Use Mermaid for most diagrams
- Add alt text for accessibility
- Include source code for complex diagrams
- Export to SVG for high-quality rendering

## Content Guidelines

### Writing Style
- Write in clear, concise English
- Use active voice when possible
- Define technical terms on first use
- Include examples for complex concepts

### Code Examples
- Provide complete, runnable examples
- Include error handling where appropriate
- Use realistic data/scenarios
- Sync with actual implementation

### Cross-References
- Link to related documents
- Maintain bidirectional links where helpful
- Use consistent terminology across documents

## Automation Rules

### Markdownlint Configuration

- Line length: 120 characters max
- Consistent list formatting
- Required language tags for code blocks
- Single trailing newline

### Validation Requirements

- All code snippets must compile/execute
- All internal links must resolve
- All diagrams must render correctly
- All external links must be accessible

## Maintenance

### Regular Updates

- Verify examples quarterly or on major releases
- Update diagrams when architecture changes
- Review and update cross-references
- Check external links for validity

### Version Control

- Include commit SHA in "Last verified" footer
- Update documentation in same PR as code changes
- Use descriptive commit messages for doc changes

---

*Last verified: 2025-01-02 - Commit: initial*