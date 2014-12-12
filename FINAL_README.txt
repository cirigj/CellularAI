This project is meant to be run out of Unity (2D mode) and NOT built.
Parameters have been made easily manipulable from the inspector.
Object named "CellControl" in the hierarchy contains all scripts that control simulation.

Scripts:

EnvironmentScript - Contains lots of static variables.
    Do NOT change values in this script without knowing first what they do.
    They have been carefully balanced and should not be changed at runtime.
    There are too many variables to explain exactly what each one does--
    --and it's honestly not all THAT important to run the simulation.
    
CellBehaviorScript - Each cell has a copy of this script.
    Script contains physical and behavioural traits for each cell.
    These traits are set with the genetic algorithm:
    PHYSICAL:
    Sugar Intake Speed (intakeSpeed)
    Sugar-to-Energy Processing Speed (processingSpeed)
    Sugar Capacity (sugarCapacity)
    Max Movement Speed (maxMovementSpeed)
    Energy Use Efficiency (useEfficiency)
    Energy Capacity (energyCapacity)
    BEHAVIOURAL:
    Courage - Willingness to fight stronger cells
    Hostility - Willingness to attack other cells
    Cowardice - Willingness to flee (without provocation)
    Greed - Willingness to pursue sugar
    
CellSpawning - Script that controls GA as well as GA parameters.
    Parameters can be changed mid-simulation, but only effect generations that 
    are created AFTER the changes have been made.
    PARAMETERS:
    A whole mess of maxes and mins. Don't worry about changing these.
    Cell and Sugar Cube prefab references (again, do not change).
    Cells Per Generation - exactly what it sounds like (must be >=2 to function properly).
    Sugar Per Generation - creates n sugar cubes with each generation.
    Sugar Min and Max - boundaries for sugar cube potency.
    Time Between Generations - self-explanatory
    Master Mutation Rate - recommend somewhere between 0 and 5.
    Constrain Genes - if true, variables cannot exceed max or min upon cell generation 
                      (recommend true, as false sort of breaks things after a while)
    Wait Min Secs - simulation will wait the specified number of seconds before choosing parents.
    Min Secs Til Next Gen - see above
    Load First Gen From File - uses filename from SaveLoad component
    Save Gens To File - same as above
    Print Average Behavior - prints averages to debug log with each new generation
    
SaveLoad - determines save file location (in project folder)
    recommend end filename with ".txt"