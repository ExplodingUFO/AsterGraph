# Phase 9: Native Avalonia Host Integration - Research

## Summary

The highest-value native-host issues are already clear from the review:

1. full-shell/root shortcut capture should be host-controllable
2. wheel and panning behavior should cooperate with host scroll/input semantics
3. keyboard-triggered menus should not be pointer-anchored
4. stock graph elements need a more coherent keyboard/focus story

Phase 9 should solve those with incremental behavior controls and better menu/focus semantics, not a visual redesign.
