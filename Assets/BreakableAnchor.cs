using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableAnchor : BreakableAnchoredChild
{
    public List<BreakableAnchoredChild> children;

    //if a child is broken
        //remove the child from the anchor
        //split the child    
        //if no children remain on the anchor
            //split the anchor

    //if an anchor is broken
        //remove all children from the anchor
        //if any removed child has no anchors
            //split the child
            
}
