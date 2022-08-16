  _____             _   ___                           _             
 |_   _|____  _____| | |_ _|_ __  ___ _ __   ___  ___| |_ ___  _ __ 
   | |/ _ \ \/ / _ \ |  | || '_ \/ __| '_ \ / _ \/ __| __/ _ \| '__|
   | |  __/>  <  __/ |  | || | | \__ \ |_) |  __/ (__| || (_) | |   
   |_|\___/_/\_\___|_| |___|_| |_|___/ .__/ \___|\___|\__\___/|_|   
   Paul Gerla, (c) 2020              |_|                            
        

Description

    The Texel Inspector allows for easy definition of and comparison of texel densities.
    Enter a target texel density (X pixels per Y meters) to see a world-space visualization
    of that density, and then quickly switch between the preview and a display of the actual
    texel density of the scene. Mipmaps are color-coded by size and matched between the
    target visualization and actual density visualization so it's easy to identify areas
    that are too dense or too sparse relative to each other or to the desired density. 
    Texture details such as currently rendering mipmap level and base texture resolution 
    are displayed in the texture for easy reference and debugging.
    
    The tool only modifies the currently open scenes to allow for the preview, and will 
    automatically revert those changes when the window is closed, the scene is closed, play
    mode is entered, or just before the scene is saved.

    To open the tool, navigate to Window -> Analysis -> Texel Inspector.
    
    It's generally easier to view the visualizations with lighting and effects disabled in
    the scene view.
    
    
 Settings Guide
 
    Base Unit in Meters: 
        The size of the unit to measure against, in meters. If this is set to 2, and Texels
        Per Base Unit is set to 1024, every 2 meters will contain 1024 pixels of texture 
        data. This is only visible in "Show Target Density" mode, and has no effect in 
        "Show Actual Density" mode.
    
    Texels Per Base Unit: 
        The desired texel count across a base unit.
           
    Checkers Per Base Unit: 
        This controls the number of the smaller dark and light checkers per base unit. Can 
        be set to 1 for no checkers.
            
    Texture Size Gradient: 
        This gradient is used for generating the texture-resolution colors used by this 
        tool, from 16px on the left to 8192px on the right. Clicking the "Transfer 
        Gradients to Swatches" button will apply the gradient colors to the swatches below.
        These swatches can also be individually edited by clicking on them.
        
    Color Each Mipmap: 
        If this option is disabled, the mipmaps of the Actual Density mode will all be 
        colored based on the base texture size. This mode is useful for seeing absolute 
        texture sizes across an environment.
        
        If this option is enabled each mipmap of each texture will be colored according to 
        its actual size. E.g., the first mip of a 1024x1024 texture will be colored as a 
        512x512 texture. This mode is useful for comparing texel densities against each 
        other and is the default setting.
        
    Show Uniform Density:
        This overrides the scene view shader with a visualization of how the texel density
        would appear if it was uniform across the entire scene. This view is not very useful
        on its own, but quite useful as a comparison tool combined with Show Actual Density.
        
    Show Actual Density:
        This replaces all materials in the open scenes with a copy of that material with 
        overwritten albedo textures of the same size as the original. A game object named 
        TexelDensityDataStorageObject is spawned in each scene containing a list of all the
        replacements. The changes are reverted and the game object is automatically 
        destroyed when entering play mode, saving the scene, closing the Texel Inspector 
        window, or closing the scene.
        
    Hide All Displays:
        Turns off Show Uniform Density and undoes the changes made by Show Actual Density.
        

Known Issues

    In the HDRP the Show Uniform Density can become extremely bright. Disabling lighting in
    the scene view should fix this.
    
    Certain naming conventions can confuse the tool. Currently it first attempts to replace
    any texture property containing these strings: "albedo", "diffuse", "base", "color",
    "main", or "emission." If any of these are found, it will replace them and move on. If
    none are found, it will replace any other textures, unless they contain the strings: 
    "metallic", "ao", "ambient", "bump", "normal", "nrm", "detail." If a property contains
    both the preferred and banned strings it will discard that as well. These strings can be
    modified in the MaterialModifierUitility.cs script, and better customizablity is
    planned for future updates.
        
        
Feedback

    Email - Pawige@gmail.com
    Twitter - https://twitter.com/Pawige
    
    Please feel free to contact me with questions, issues, requests, a project you used
    this for, or just to say hi!