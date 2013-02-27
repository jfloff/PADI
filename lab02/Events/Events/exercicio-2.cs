// A posi��o do slider � actualizada com a nova posi��o apenas se esta for v�lida.
// Para validar a posi��o, o slider desencadeia um evento antes de fazer a atribui��o da posi��o.
// Deve ser adicionada ao evento uma callback tal que, se a nova posi��o for maior que 50, ent�o � inv�lida.
// Na classe Slider, sugere-se a cria��o do evento Move e a actualiza��o da propriedade Position.
// Recomenda-se a cria��o da classe MoveEventArgs para transfer�ncia de argumentos.

using System;
// este delegate e' a base para o event Move do slider

class Slider {
	private int position;
    public delegate bool MoveEventHandler(object source, MoveEventArgs e);
    public event MoveEventHandler sliderMoved;
    
    public bool OnSliderMove(int newPosition)
    {
        // If there exist any subscribers call the event
        if (sliderMoved != null)
        {
            return sliderMoved(this, new MoveEventArgs(newPosition));
        }

        return true;
    }

	public int Position {
		get {
			return position;
		}
	    // e' este bloco que e' executado quando se move o slider
		set {
            if (OnSliderMove(value))
                this.position = value;
		}
	}
}

// esta  classe contem os argumentos do evento move do slider
public class MoveEventArgs : EventArgs
{
    public int FinalMove { get; internal set; }

    public MoveEventArgs(int FinalMove)
    {
        this.FinalMove = FinalMove;
    }
}

class Form {
	static void Main( ) {
		Slider slider = new Slider( );

		// TODO: register with the Move event
        // internal delegate needs a 'new' --> is in fact only 'newing' a event handler
        slider.sliderMoved += new Slider.MoveEventHandler(slider_Move);

	    // estas sao as duas alteracoes simuladas no slider
		slider.Position = 20;
        Console.WriteLine("Position = " + slider.Position);
		slider.Position = 60;
        Console.WriteLine("Position = " + slider.Position);

        Console.Read();
	}

	// este � o m�todo que deve ser chamado quando o slider e' movido
	static bool slider_Move(object source, MoveEventArgs args) {
        if (args.FinalMove > 50)
            return false;
        else 
            return true;
	}
}
