using System;
// este delegate e' a base para o event Move do slider
delegate void MoveEventHandler(object source, MoveEventArgs e);

// esta  classe contem os argumentos do evento move do slider
public class MoveEventArgs : EventArgs {
	// ...
}


class Slider {
	private int position;

	public int Position {
		get {
			return position;
		}
	// e' este bloco que e' executado quando se move o slider
		set {
			position = value;
		}
	}
}


class Form {
	static void Main( ) {
		Slider slider = new Slider( );

		// TODO: register with the Move event

	// estas sao as duas alteracoes simuladas no slider
		slider.Position = 20;
		slider.Position = 60;
	}

	// este é o método que deve ser chamado quando o slider e' movido
	static void slider_Move(object source, MoveEventArgs e) {
		// TODO
	}
}
