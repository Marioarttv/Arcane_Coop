# Picture Explanation Challenge - Image Requirements

## Overview
The Picture Explanation Challenge is a voice chat-based cooperative game where players describe and identify Arcane-themed images. One player describes images over Discord voice chat, then hides the image, while the other player selects from 4 visual choices.

## Required Directory Structure
All images should be placed in: `/wwwroot/images/pictures/`

## Required Images

### Technology Category
- **hextech_crystal.jpg**: A glowing bright blue hextech crystal with intricate geometric patterns and sharp angular edges
  - *Voice-friendly features: Bright blue glow, geometric/angular shape, crystalline structure*
- **shimmer_crystal.jpg**: A purple-pink shimmer crystal with organic curves (distractor)
  - *Voice-friendly features: Purple-pink color, more rounded/organic shape*
- **zaun_pipe.jpg**: Industrial metal pipe from Zaun with green chemical residue and rust (distractor)
  - *Voice-friendly features: Metallic gray pipe, green chemical stains, industrial/rusty texture*
- **piltover_gear.jpg**: Golden mechanical gear from Piltover technology with precise teeth and clean finish (distractor)
  - *Voice-friendly features: Golden color, circular gear shape with teeth, clean/polished appearance*

### Location Category
- **zaun_undercity.jpg**: Dark underground city with green chemical lighting, industrial pipes, and shadowy atmosphere
  - *Voice-friendly features: Dark/shadowy setting, green glowing lights, lots of pipes and industrial elements*
- **piltover_academy.jpg**: Elegant white and gold Piltover Academy building with clean architecture (distractor)
  - *Voice-friendly features: White/gold colors, clean elegant architecture, bright daylight*
- **bridge_progress.jpg**: The Bridge of Progress - large stone bridge connecting upper and lower cities (distractor)
  - *Voice-friendly features: Massive bridge structure, stone construction, spans across a gap*
- **hexgate.jpg**: The hexgate portal structure with blue energy rings and technological framework (distractor)
  - *Voice-friendly features: Circular portal rings, bright blue energy, high-tech metal framework*
- **piltover_council.jpg**: Elegant golden council chamber with circular tiered seating and ornate decorative architecture
  - *Voice-friendly features: Golden interior, circular room layout, ornate decorative details, formal seating arrangement*
- **zaun_factory.jpg**: Industrial Zaun factory with multiple smokestacks and dark pollution (distractor)
  - *Voice-friendly features: Multiple tall smokestacks, dark smoke/pollution, industrial factory buildings*

### Weapon Category
- **vi_gauntlets.jpg**: Large oversized metal gauntlets with bright blue energy cores and visible mechanical joints/pistons
  - *Voice-friendly features: Massive size, blue glowing energy cores, mechanical details like joints and pistons*
- **jayce_hammer.jpg**: Jayce's transforming mercury hammer with golden accents and sleek design (distractor)
  - *Voice-friendly features: Hammer shape, golden/bronze coloring, sleek refined appearance*
- **caitlyn_rifle.jpg**: Caitlyn's elegant hextech sniper rifle with blue energy and precision scope (distractor)
  - *Voice-friendly features: Long rifle shape, blue hextech elements, precision scope, elegant design*
- **viktor_staff.jpg**: Viktor's mechanical staff with purple hexcore and technological augmentations (distractor)
  - *Voice-friendly features: Staff/pole shape, purple energy core, mechanical/technological details*

### Chemistry Category
- **shimmer_vial.jpg**: Small glass vial containing glowing purple-pink shimmer liquid with swirling effects
  - *Voice-friendly features: Small vial shape, bright purple-pink glow, swirling liquid motion, glass container*
- **hextech_potion.jpg**: Blue glowing hextech potion in a geometric bottle with clean lines (distractor)
  - *Voice-friendly features: Bright blue glow, geometric/angular bottle shape, clean technical appearance*
- **zaun_medicine.jpg**: Green chemical medicine in a rough bottle with industrial labeling (distractor)
  - *Voice-friendly features: Green color, rough/industrial bottle design, medical/chemical appearance*
- **healing_elixir.jpg**: Generic healing potion with golden sparkles and traditional bottle shape (distractor)
  - *Voice-friendly features: Golden color with sparkles, traditional round bottle shape, magical appearance*

## Image Specifications
- **Format**: JPG preferred for smaller file sizes
- **Resolution**: 400x300 pixels minimum for clear visibility
- **Aspect Ratio**: 4:3 recommended for consistent display
- **File Size**: Keep under 200KB each for fast loading
- **Quality**: High enough to distinguish details for descriptions
- **Style**: Should match Arcane's art style and color palette

## Color Palette Guidelines
- **Piltover**: Golden (#c89b3c), white, clean blues
- **Zaun**: Teal (#00c8c8), greens, industrial grays, shimmer purple/pink
- **General**: Maintain the contrast between upper and lower city aesthetics

## Gameplay Context
- **Piltover Player (Caitlyn)**: Views one main image and describes it over Discord voice chat
- **Strategic Element**: Image disappears once player presses "Finished Describing" - no second chances!
- **Zaunite Player (Vi)**: Listens over voice chat, then selects from 4 choice images
- **Communication-Focused**: Designed for natural voice communication, not text descriptions
- **High Stakes**: Players must describe accurately the first time since image becomes unavailable

## Image Design Requirements
- **Voice-Friendly Details**: Images should have describable elements (colors, shapes, objects, lighting)
- **Distinctive Features**: Each image needs unique characteristics that can be communicated verbally
- **Clear Differentiation**: Distractors should be similar enough to be plausible but different enough to distinguish through good description
- **Avoid Text**: Minimize text elements since players communicate over voice, not by reading

## Voice Chat Optimization
- **Describable Elements**: Focus on colors, lighting, shapes, character poses, background details
- **Unique Identifiers**: Each image should have 2-3 distinctive features that are easy to verbally communicate
- **Lighting Cues**: Strong lighting differences help with voice descriptions ("glowing blue", "dark shadows", "golden light")
- **Object Placement**: Clear foreground/background elements aid description ("in the center", "on the left side", "in the background")

## Technical Notes
- Images displayed in responsive grid layout for choice selection
- Main image hidden permanently after "Finished Describing" button press
- Hover effects and selection highlighting applied via CSS
- Fallback handling exists for missing images (displays placeholder path)
- No text overlay or captions needed since communication is voice-based