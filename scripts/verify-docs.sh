#!/bin/bash

# 🔍 Script de Verificación de Snippets de Código en Documentación
# Extrae y compila ejemplos de C# de los archivos markdown

set -e

DOC_DIR="docs"
SRC_DIR="src"  
TEMP_DIR="/tmp/doc-verify"
COMPILE_ERRORS=0

echo "🔍 Iniciando verificación de snippets de código..."

# Limpiar directorio temporal
rm -rf "$TEMP_DIR"
mkdir -p "$TEMP_DIR"

# Función para extraer snippets de C#
extract_csharp_snippets() {
    local file="$1"
    local output_dir="$2"
    local counter=1
    
    # Extraer bloques de código C#
    awk -v output_dir="$output_dir" -v filename="$(basename "$file" .md)" '
        /^```csharp/ {in_code=1; counter++; next}
        /^```/ && in_code {in_code=0; next}
        in_code {print > output_dir"/"filename"_snippet_"counter".cs"}
    ' "$file"
}

# Función para compilar snippet
compile_snippet() {
    local snippet_file="$1"
    local snippet_name="$(basename "$snippet_file")"
    
    echo "  📝 Verificando: $snippet_name"
    
    # Crear proyecto temporal para compilar el snippet
    local temp_project="$TEMP_DIR/${snippet_name%.*}"
    mkdir -p "$temp_project"
    
    # Crear archivo .csproj básico
    cat > "$temp_project/TempProject.csproj" << EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.App" />
    <PackageReference Include="MediatR" Version="12.0.1" />
    <PackageReference Include="FluentValidation" Version="11.5.1" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.0" />
  </ItemGroup>
</Project>
EOF

    # Wrap el snippet en una clase válida si es necesario
    local wrapped_content
    if grep -q "class\|interface\|namespace" "$snippet_file"; then
        wrapped_content="$(cat "$snippet_file")"
    else
        wrapped_content="using System;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace TempNamespace 
{
    public class TempClass 
    {
$(cat "$snippet_file" | sed 's/^/        /')
    }
}"
    fi
    
    echo "$wrapped_content" > "$temp_project/Program.cs"
    
    # Intentar compilar
    if dotnet build "$temp_project" --verbosity quiet --nologo > /dev/null 2>&1; then
        echo "    ✅ Compilación exitosa"
        return 0
    else
        echo "    ❌ Compilación falló"
        echo "    📄 Contenido del snippet:"
        sed 's/^/       /' "$snippet_file"
        echo ""
        COMPILE_ERRORS=$((COMPILE_ERRORS + 1))
        return 1
    fi
}

# Función principal de verificación
verify_documentation() {
    echo "📂 Buscando archivos de documentación..."
    
    local doc_files
    doc_files=$(find "$DOC_DIR" -name "*.md" -type f)
    
    if [ -z "$doc_files" ]; then
        echo "⚠️  No se encontraron archivos de documentación"
        return 1
    fi
    
    local total_files=0
    local files_with_snippets=0
    
    while IFS= read -r doc_file; do
        total_files=$((total_files + 1))
        echo "📄 Procesando: $doc_file"
        
        # Crear directorio para snippets de este archivo
        local snippet_dir="$TEMP_DIR/$(basename "$doc_file" .md)"
        mkdir -p "$snippet_dir"
        
        # Extraer snippets
        extract_csharp_snippets "$doc_file" "$snippet_dir"
        
        # Verificar si hay snippets
        local snippets
        snippets=$(find "$snippet_dir" -name "*.cs" -type f 2>/dev/null || true)
        
        if [ -n "$snippets" ]; then
            files_with_snippets=$((files_with_snippets + 1))
            echo "  🔍 Encontrados $(echo "$snippets" | wc -l) snippets de C#"
            
            # Compilar cada snippet
            while IFS= read -r snippet; do
                compile_snippet "$snippet"
            done <<< "$snippets"
        else
            echo "  ℹ️  Sin snippets de C#"
        fi
        
        echo ""
    done <<< "$doc_files"
    
    echo "📊 Resumen de verificación:"
    echo "  📁 Archivos procesados: $total_files"
    echo "  📝 Archivos con snippets: $files_with_snippets"
    echo "  ❌ Errores de compilación: $COMPILE_ERRORS"
    
    return $COMPILE_ERRORS
}

# Función para verificar comandos bash
verify_bash_commands() {
    echo "🔍 Verificando comandos bash en documentación..."
    
    local bash_errors=0
    
    # Buscar bloques de bash y verificar comandos básicos
    find "$DOC_DIR" -name "*.md" -exec grep -l '```bash' {} \; | while read -r file; do
        echo "📄 Verificando comandos bash en: $file"
        
        # Extraer comandos dotnet
        grep -A 20 '```bash' "$file" | grep -E '^dotnet ' | while read -r cmd; do
            echo "  🔧 Verificando: $cmd"
            
            # Verificar sintaxis básica de dotnet
            if echo "$cmd" | grep -qE '^dotnet (build|test|restore|run)'; then
                echo "    ✅ Comando válido"
            else
                echo "    ⚠️  Comando no reconocido: $cmd"
                bash_errors=$((bash_errors + 1))
            fi
        done
    done
    
    return 0
}

# Verificar que dotnet esté disponible
if ! command -v dotnet &> /dev/null; then
    echo "❌ dotnet CLI no encontrado. Instale .NET 8.0 SDK"
    exit 1
fi

echo "🔧 Dotnet version: $(dotnet --version)"
echo ""

# Ejecutar verificaciones
verify_documentation
doc_result=$?

verify_bash_commands
bash_result=$?

# Limpiar
rm -rf "$TEMP_DIR"

# Resultado final
echo ""
if [ $doc_result -eq 0 ] && [ $bash_result -eq 0 ]; then
    echo "✅ Verificación completada sin errores"
    exit 0
else
    echo "❌ Verificación completada con errores"
    echo "   Errores de compilación C#: $COMPILE_ERRORS"
    exit 1
fi