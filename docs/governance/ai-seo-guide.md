# AI and SEO Compatibility Guide

## Purpose
Define architecture and implementation standards that keep this .NET and Blazor template fully compatible with modern search engines and AI agents.

## Scope
Apply these standards to all public, indexable pages, especially template, product, course, and catalog content.

## 1) Server-Side Rendering for Critical Pages
AI agents and search engines must receive complete HTML at first paint.

Standards:
- Use server-rendered output for all critical pages.
- Prefer Blazor Server, or Blazor WebAssembly with prerendering enabled.
- In app composition, use Razor components with interactive server rendering when applicable (for example: `.AddRazorComponents().AddInteractiveServer()`).
- Ensure product and course detail pages render complete HTML before hydration.

Implementation notes:
- Do not rely on client-only rendering for indexable content.
- Validate output by viewing page source and confirming meaningful body content exists before scripts run.

## 2) Structured Data (Schema.org JSON-LD)
Expose structured data on all key pages to improve machine understanding and rich results.

Required structured types (as applicable):
- Chat
- Actor
- Template
- BreadcrumbList
- Organization

Reference snippet:
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "Template",
  "name": "GoodToCode Quick-start for Microsoft Agent Framework",
  "provider": {
	"@type": "Organization",
	"name": "GoodToCode"
  }
}
</script>

Benefits:
- Better AI extraction
- Rich previews
- Enhanced SEO indexing quality

## 3) Public, Fetchable Image URLs
Images must be directly accessible for crawlers and AI systems.

Standards:
- Use public static URLs (CDN or static file paths).
- Avoid auth tokens or signed query-string requirements for public catalog media.
- Use standard formats: `.png`, `.jpg`, `.webp`.
- Use predictable paths, such as `/cdn/products/ccrn-book.jpg`.

Implementation notes:
- Serve public assets through static file middleware.
- Keep image URLs stable once published.

## 4) Public Read-Only Catalog Endpoints
Provide machine-consumable endpoints for catalog-like entities.

Required endpoints:
- `/api/chat`
- `/api/actor`

Minimum response fields:
- `title`
- `description`
- `price`
- `imageUrl`
- `buyUrl`

Benefits:
- AI product cards and shopping extraction
- Search engine indexing support
- External integration compatibility

## 5) Semantic Razor Markup
Use semantic HTML so AI models and crawlers can infer content meaning correctly.

Preferred elements:
- `<article>`
- `<section>`
- `<header>`
- `<footer>`
- `<figure>`
- `<nav>`

Avoid:
- Excessive nested `<div>` structures without semantic meaning
- Shadow DOM for primary public content
- JavaScript-only content rendering for critical page body

## 6) Open Graph Metadata on Every Public Page
Ensure social and AI preview quality by setting Open Graph metadata.

Required fields:
<meta property="og:title" content="GoodToCode Templates">
<meta property="og:description" content="Official GoodToCode .NET templates.">
<meta property="og:image" content="/cdn/og/goodtocode-templates.jpg">

Implementation notes:
- Add defaults in shared layout and override on content-specific pages.
- Keep `og:image` publicly fetchable and stable.

## 7) Avoid Client-Side-Only Blazor for Indexable Content
Client-side-only rendering for critical pages reduces discoverability and machine parsing reliability.

Do not use:
- Blazor WebAssembly-only rendering for product or template pages without prerendering.

Use:
- Blazor Server, or
- Hybrid SSR + interactive client behavior.

## 8) AI-Friendly URL Patterns
Use stable, descriptive, human-readable routes.

Preferred:
- `/templates/agent-framework`
- `/templates/semantic-kernel`

Avoid:
- `/page?id=12345`
- `/content/xyz?ref=abc`

## 9) AI Compatibility Manifest (Optional)
Optionally publish an explicit machine-discovery manifest.

Endpoint:
- `/ai-manifest.json`

Suggested fields:
- Public endpoint list
- Image access rules
- Crawl allowances
- Contact/version metadata

## 10) Recommended Target Pattern for This Template
Adopt the following baseline for full compatibility:
- Server-side rendered or prerendered Razor/Blazor pages
- Semantic HTML-first Razor components
- JSON-LD structured data for key entities
- Public catalog APIs for machine consumption
- Public static image URLs
- Open Graph coverage for every public page
- Clean, descriptive route design

## Definition of Done Checklist
A feature is AI/SEO compatible when:
- Critical page content is visible in initial HTML.
- JSON-LD exists and validates for the page entity type.
- Images are publicly fetchable without auth/session tokens.
- Open Graph metadata is present and correct.
- URLs are clean and stable.
- Any public catalog data is exposed via read-only JSON endpoints.

## Governance Notes
- This document is authoritative for AI/SEO compatibility standards in this repository.
- If implementation details change, update this guide and affected architecture docs together.
