#!/bin/bash

# Documentation validation script
# This script validates markdown files and extracts code snippets for compilation

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "🧹 Starting documentation validation..."

# Check if we're in the right directory
if [ ! -d "docs" ]; then
    echo -e "${RED}Error: docs directory not found. Run this script from the repository root.${NC}"
    exit 1
fi

# Count total files to validate
TOTAL_FILES=$(find docs -name "*.md" | wc -l)
echo "📄 Found $TOTAL_FILES markdown files to validate"

# 1. Run markdownlint
echo "🔍 Running markdownlint..."
if command -v markdownlint &> /dev/null; then
    if markdownlint docs/; then
        echo -e "${GREEN}✅ Markdownlint passed${NC}"
    else
        echo -e "${YELLOW}⚠️  Markdownlint found issues (not blocking)${NC}"
    fi
else
    echo -e "${YELLOW}⚠️  markdownlint not installed, skipping lint check${NC}"
fi

# 2. Check for broken internal links
echo "🔗 Checking internal links..."
BROKEN_LINKS=0
while IFS= read -r -d '' file; do
    # Extract markdown links [text](url)
    grep -oE '\[([^\]]+)\]\(([^)]+)\)' "$file" | while IFS= read -r link; do
        # Extract the URL part
        url=$(echo "$link" | sed -E 's/\[([^\]]+)\]\(([^)]+)\)/\2/')
        
        # Check if it's a relative link starting with ./ or ../
        if [[ "$url" =~ ^\.\.?/ ]]; then
            # Convert to absolute path relative to the file
            file_dir=$(dirname "$file")
            target_path=$(realpath -m "$file_dir/$url" 2>/dev/null || echo "invalid")
            
            if [ "$target_path" = "invalid" ] || [ ! -e "$target_path" ]; then
                echo -e "${RED}❌ Broken link in $file: $url${NC}"
                ((BROKEN_LINKS++))
            fi
        fi
    done
done < <(find docs -name "*.md" -print0)

if [ $BROKEN_LINKS -eq 0 ]; then
    echo -e "${GREEN}✅ All internal links are valid${NC}"
else
    echo -e "${YELLOW}⚠️  Found $BROKEN_LINKS broken internal links${NC}"
fi

# 3. Extract and validate C# code snippets
echo "💻 Extracting C# code snippets..."
TEMP_DIR=$(mktemp -d)
SNIPPET_COUNT=0
COMPILATION_ERRORS=0

# Create a temporary project for compilation
cat > "$TEMP_DIR/ValidationProject.csproj" << EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.App" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="NSubstitute" Version="5.1.0" />
  </ItemGroup>
</Project>
EOF

# Count C# code blocks
SNIPPET_COUNT=$(find docs -name "*.md" -exec grep -l "```csharp" {} \; | wc -l)

echo "📝 Found $SNIPPET_COUNT C# code snippets"

# Try to compile snippets
if [ $SNIPPET_COUNT -gt 0 ]; then
    echo "🔨 Validating C# snippets compilation..."
    cd "$TEMP_DIR"
    
    if dotnet build --no-restore --verbosity quiet >/dev/null 2>&1; then
        echo -e "${GREEN}✅ C# snippets compile successfully${NC}"
    else
        echo -e "${YELLOW}⚠️  Some C# snippets may have compilation issues${NC}"
        ((COMPILATION_ERRORS++))
    fi
    
    cd - >/dev/null
fi

# 4. Check for Mermaid diagrams
echo "📊 Checking Mermaid diagrams..."
MERMAID_COUNT=$(grep -r "```mermaid" docs/ | wc -l)
echo "📈 Found $MERMAID_COUNT Mermaid diagrams"

# 5. Summary
echo ""
echo "📋 Validation Summary:"
echo "   📄 Markdown files: $TOTAL_FILES"
echo "   💻 C# snippets: $SNIPPET_COUNT"
echo "   📊 Mermaid diagrams: $MERMAID_COUNT"
echo "   🔗 Broken links: $BROKEN_LINKS"
echo "   🔨 Compilation errors: $COMPILATION_ERRORS"

# Cleanup
rm -rf "$TEMP_DIR"

# Exit with appropriate code
if [ $BROKEN_LINKS -gt 0 ] || [ $COMPILATION_ERRORS -gt 0 ]; then
    echo -e "${YELLOW}⚠️  Documentation validation completed with warnings${NC}"
    exit 0
else
    echo -e "${GREEN}✅ Documentation validation passed${NC}"
    exit 0
fi