# 📚 Makefile para Documentación - Conaprole Orders API

.PHONY: help doc-lint doc-verify doc-build doc-clean doc-all

# Variables
DOC_DIR := docs
SCRIPTS_DIR := scripts

help: ## 📋 Muestra esta ayuda
	@echo "📚 Comandos disponibles para documentación:"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-20s\033[0m %s\n", $$1, $$2}'
	@echo ""

doc-lint: ## 🔍 Ejecuta markdownlint en toda la documentación
	@echo "🔍 Ejecutando markdownlint..."
	@markdownlint-cli2 "$(DOC_DIR)/**/*.md"
	@echo "✅ Lint completado"

doc-lint-fix: ## 🔧 Ejecuta markdownlint con auto-fix
	@echo "🔧 Ejecutando markdownlint con auto-fix..."
	@markdownlint-cli2 "$(DOC_DIR)/**/*.md" --fix
	@echo "✅ Auto-fix completado"

doc-verify: ## ✅ Verifica que los snippets de código compilen
	@echo "✅ Verificando snippets de código..."
	@$(SCRIPTS_DIR)/verify-docs.sh
	@echo "✅ Verificación completada"

doc-build: ## 🏗️ Construye el proyecto para validar ejemplos
	@echo "🏗️ Construyendo proyecto..."
	@dotnet build Conaprole.Orders.sln --configuration Release --verbosity minimal
	@echo "✅ Build completado"

doc-test: ## 🧪 Ejecuta tests para validar ejemplos funcionales
	@echo "🧪 Ejecutando tests..."
	@dotnet test Conaprole.Orders.sln --configuration Release --verbosity minimal --logger "console;verbosity=minimal"
	@echo "✅ Tests completados"

doc-clean: ## 🧹 Limpia archivos temporales de documentación
	@echo "🧹 Limpiando archivos temporales..."
	@rm -rf /tmp/doc-verify
	@find $(DOC_DIR) -name "*.tmp" -delete || true
	@echo "✅ Limpieza completada"

doc-stats: ## 📊 Muestra estadísticas de documentación
	@echo "📊 Estadísticas de documentación:"
	@echo ""
	@echo "📁 Archivos por categoría:"
	@find $(DOC_DIR) -name "*.md" | cut -d'/' -f2 | sort | uniq -c | awk '{printf "  %-15s %d archivos\n", $$2, $$1}'
	@echo ""
	@echo "📝 Total de archivos: $$(find $(DOC_DIR) -name "*.md" | wc -l)"
	@echo "📏 Total de líneas: $$(find $(DOC_DIR) -name "*.md" -exec wc -l {} + | tail -1 | awk '{print $$1}')"
	@echo "🔤 Total de palabras: $$(find $(DOC_DIR) -name "*.md" -exec wc -w {} + | tail -1 | awk '{print $$1}')"

doc-all: doc-clean doc-lint doc-build doc-verify ## 🎯 Ejecuta todas las validaciones de documentación
	@echo ""
	@echo "🎉 Todas las validaciones de documentación completadas exitosamente!"

# Comandos específicos para desarrollo
doc-watch: ## 👀 Observa cambios en documentación (requiere entr)
	@echo "👀 Observando cambios en documentación..."
	@echo "💡 Presiona Ctrl+C para detener"
	@find $(DOC_DIR) -name "*.md" | entr -c make doc-lint

doc-serve: ## 🌐 Sirve documentación localmente (requiere Python)
	@echo "🌐 Sirviendo documentación en http://localhost:8000"
	@cd $(DOC_DIR) && python3 -m http.server 8000

# Comandos para CI/CD
ci-docs: doc-lint doc-verify ## 🤖 Validaciones para CI/CD
	@echo "🤖 Validaciones de CI/CD completadas"

# Información del sistema
doc-info: ## ℹ️ Información del sistema de documentación
	@echo "ℹ️  Información del sistema:"
	@echo "  📂 Directorio docs: $(DOC_DIR)"
	@echo "  🔧 Scripts: $(SCRIPTS_DIR)"
	@echo "  📏 Markdownlint: $$(markdownlint-cli2 --version 2>/dev/null || echo 'No instalado')"
	@echo "  🏗️ .NET: $$(dotnet --version 2>/dev/null || echo 'No instalado')"
	@echo "  🐧 SO: $$(uname -s) $$(uname -r)"