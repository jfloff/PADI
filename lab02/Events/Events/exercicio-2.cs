// A posição do slider é actualizada com a nova posição apenas se esta for válida.
// Para validar a posição, o slider desencadeia um evento antes de fazer a atribuição da posição.
// Deve ser adicionada ao evento uma callback tal que, se a nova posição for maior que 50, então é inválida.
// Na classe Slider, sugere-se a criação do evento Move e a actualização da propriedade Position.
// Recomenda-se a criação da classe MoveEventArgs para transferência de argumentos.

using System;
// este delegate e' a base para o event Move do slider
public delegate void MoveEventHandler(object source, MoveEventArgs e);

// esta  classe contem os argumentos do evento move do slider
public class MoveEventArgs : EventArgs {
    int toMove;

    public MoveEventArgs(int toMove) 
    {
        this.toMove = toMove; 
    }

    public int ToMove
    {
        set { toMove = value; }
        get { return toMove; }
    }
}

public class Listener
{
    private MoveEventHandler _e; // delegate com lista de subscritores
    public event MoveEventHandler E
    {
        add { 
            _e += value; 
        }
        
        remove { 
            if (_e != null) { 
                _e -= value; 
            } 
        }
    }
}


class Slider {
	private int position;
    public event MoveEventHandler E;
    public MoveEventArgs args = null;

	public int Position {
		get {
			return position;
		}
	// e' este bloco que e' executado quando se move o slider
		set {
			position = value;
            E(this, args);
		}
	}
}

class Form {
	static void Main( ) {
		Slider slider = new Slider( );

		// TODO: register with the Move event
        slider.E += new MoveEventHandler(slider_Move);

	    // estas sao as duas alteracoes simuladas no slider
		slider.Position = 20;
		slider.Position = 60;

        Console.Read();
	}

	// este é o método que deve ser chamado quando o slider e' movido
	static void slider_Move(object source, MoveEventArgs e) {
        Console.WriteLine("It's ALIVE!");
	}
}
