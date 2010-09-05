using System;
using CPU = Cosmos.Compiler.Assembler.X86;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using Cosmos.Compiler.Assembler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Shr )]
    public class Shr : ILOp
    {
        public Shr( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            string xLabelName = AppAssemblerNasm.TmpPosLabel(aMethod, aOpCode);
            var xStackItem_ShiftAmount = Assembler.Stack.Pop();
            var xStackItem_Value = Assembler.Stack.Pop();
            if( xStackItem_Value.Size <= 4 )
            {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; // shift amount
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX }; // value
                new CPUx86.Move { DestinationReg = CPUx86.Registers.CL, SourceReg = CPUx86.Registers.AL };
                new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.CL };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EBX };
                Assembler.Stack.Push( xStackItem_Value );
            }
            else if( xStackItem_Value.Size <= 8 )
            {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                new Label( xLabelName + "__StartLoop" );
                new CPUx86.Compare { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EAX };
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = xLabelName + "__EndLoop" };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.CL, SourceValue = 1 };
                new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.CL };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EBX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.CL, SourceValue = 1 };
                new CPUx86.RotateThroughCarryRight { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, Size = 32, SourceReg = CPUx86.Registers.CL };
                new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 1 };
                new CPUx86.Jump { DestinationLabel = xLabelName + "__StartLoop" };

                new Label( xLabelName + "__EndLoop" );
                Assembler.Stack.Push( xStackItem_Value );
            }
        }

    }
}