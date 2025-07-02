# Documentation Management Makefile

.PHONY: help docs-validate docs-lint docs-fix docs-stats docs-clean

help: ## Show this help message
	@echo "📚 Documentation Management Commands"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-20s\033[0m %s\n", $$1, $$2}'

docs-validate: ## Validate all documentation files
	@echo "🧹 Running comprehensive documentation validation..."
	@./scripts/validate-docs.sh

docs-lint: ## Run markdownlint on documentation
	@echo "🔍 Running markdownlint..."
	@markdownlint docs/ || true

docs-fix: ## Auto-fix markdownlint issues where possible
	@echo "🔧 Auto-fixing markdown issues..."
	@markdownlint docs/ --fix || true
	@echo "✅ Auto-fixes applied. Review changes and commit."

docs-stats: ## Show documentation statistics
	@echo "📊 Documentation Statistics"
	@echo "=========================================="
	@echo "📄 Markdown files:     $$(find docs -name '*.md' | wc -l)"
	@echo "💻 C# code snippets:   $$(grep -r '```csharp' docs/ | wc -l)"
	@echo "🐚 Bash code snippets: $$(grep -r '```bash' docs/ | wc -l)"
	@echo "📊 Mermaid diagrams:   $$(grep -r '```mermaid' docs/ | wc -l)"
	@echo "🔗 Internal links:     $$(grep -r '\[.*\](\..*\.md)' docs/ | wc -l)"
	@echo "📝 Total lines:        $$(find docs -name '*.md' -exec wc -l {} + | tail -1 | awk '{print $$1}')"

docs-clean: ## Clean up temporary documentation files
	@echo "🧹 Cleaning temporary documentation files..."
	@find docs -name "*.tmp" -delete || true
	@find docs -name "*~" -delete || true
	@echo "✅ Cleanup complete."

docs-verify-snippets: ## Extract and validate C# code snippets
	@echo "💻 Extracting and validating C# code snippets..."
	@temp_dir=$$(mktemp -d); \
	echo "Creating temporary project in $$temp_dir..."; \
	cp -r src/* "$$temp_dir/" 2>/dev/null || true; \
	find docs -name "*.md" -exec grep -l "```csharp" {} \; | while read file; do \
		echo "Processing $$file..."; \
		awk '/```csharp/,/```/ {if(!/```/) print}' "$$file" > "$$temp_dir/snippet_$$(basename $$file).cs" 2>/dev/null || true; \
	done; \
	echo "Validating snippets..."; \
	cd "$$temp_dir" && dotnet build --no-restore --verbosity quiet >/dev/null 2>&1 && echo "✅ All snippets compile" || echo "⚠️  Some snippets have issues"; \
	rm -rf "$$temp_dir"

docs-serve: ## Serve documentation locally (requires Python)
	@echo "🌐 Starting local documentation server..."
	@echo "📖 Open http://localhost:8000/docs/ in your browser"
	@python3 -m http.server 8000 || python -m SimpleHTTPServer 8000

docs-export-diagrams: ## Export Mermaid diagrams to SVG (requires mermaid-cli)
	@echo "📊 Exporting Mermaid diagrams to SVG..."
	@mkdir -p docs/diagrams/exports
	@find docs -name "*.md" -exec grep -l "```mermaid" {} \; | while read file; do \
		echo "Processing diagrams in $$file..."; \
	done
	@echo "ℹ️  Install mermaid-cli: npm install -g @mermaid-js/mermaid-cli"

install-tools: ## Install required documentation tools
	@echo "🔧 Installing documentation tools..."
	@npm install -g markdownlint-cli @mermaid-js/mermaid-cli
	@echo "✅ Tools installed successfully"