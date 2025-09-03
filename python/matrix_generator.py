import tkinter as tk
from tkinter import messagebox
import re

class MobilityMatrixCreator:
    def __init__(self, root):
        self.root = root
        self.root.title("Chess Mobility Matrix Creator")
        
        # Matrix to store values
        self.matrix = [[0 for _ in range(8)] for _ in range(8)]
        self.selected_cells: set[tuple[int, int]] = set()
        
        # Create UI
        self.create_widgets()
        
    def create_widgets(self):
        # Name field
        name_frame = tk.Frame(self.root)
        name_frame.pack(pady=10)
        
        tk.Label(name_frame, text="Matrix Name:").pack(side=tk.LEFT)
        self.name_var = tk.StringVar(value="mobilityMatrix")
        name_entry = tk.Entry(name_frame, textvariable=self.name_var, width=20)
        name_entry.pack(side=tk.LEFT, padx=5)
        
        # Input field
        input_frame = tk.Frame(self.root)
        input_frame.pack(pady=5)
        
        tk.Label(input_frame, text="Value:").pack(side=tk.LEFT)
        self.value_var = tk.StringVar()
        value_entry = tk.Entry(input_frame, textvariable=self.value_var, width=10)
        value_entry.pack(side=tk.LEFT, padx=5)
        
        set_btn = tk.Button(input_frame, text="Set Value", command=self.set_value)
        set_btn.pack(side=tk.LEFT, padx=5)
        
        # Buttons frame
        btn_frame = tk.Frame(self.root)
        btn_frame.pack(pady=10)
        
        clear_btn = tk.Button(btn_frame, text="Unselect All", command=self.unselect_all)
        clear_btn.pack(side=tk.LEFT, padx=5)
        
        import_btn = tk.Button(btn_frame, text="Import", command=self.import_from_clipboard)
        import_btn.pack(side=tk.LEFT, padx=5)
        
        output_btn = tk.Button(btn_frame, text="Output", command=self.output_matrix)
        output_btn.pack(side=tk.LEFT, padx=5)
        
        # Create grid
        self.grid_frame = tk.Frame(self.root)
        self.grid_frame.pack(pady=10)
        
        self.cells = {}
        for i in range(8):
            for j in range(8):
                cell = tk.Label(self.grid_frame, text="0", relief="raised", width=4, height=2, 
                               bg="white", font=("Arial", 10))
                cell.grid(row=i, column=j, padx=1, pady=1)
                cell.bind("<Button-1>", lambda e, row=i, col=j: self.toggle_cell(e, row, col))
                self.cells[(i, j)] = cell
    
    def toggle_cell(self, event, row, col):
        if (row, col) in self.selected_cells:
            self.selected_cells.remove((row, col))
            self.cells[(row, col)].config(bg="white")
            return
        if event.state & 0x0001 and len(self.selected_cells) == 1:
            first_cell = self.selected_cells.pop()
            second_cell = (row, col)

            min_x = min(first_cell[0], second_cell[0])
            max_x = max(first_cell[0], second_cell[0])
            min_y = min(first_cell[1], second_cell[1])
            max_y = max(first_cell[1], second_cell[1])

            for x in range(min_x, max_x + 1):
                for y in range(min_y, max_y + 1):
                    self.cells[(x, y)].config(bg="lightblue")
                    self.selected_cells.add((x, y))
            return
        if not (event.state & 0x0004):
            for cell in self.selected_cells:
                self.cells[cell].config(bg="white")
            self.selected_cells.clear()
        self.selected_cells.add((row, col))
        self.cells[(row, col)].config(bg="lightblue")

    def set_value(self):
        if not self.value_var.get():
            messagebox.showwarning("Input Error", "Please enter a value")
            return
            
        try:
            value = int(self.value_var.get())
        except ValueError:
            messagebox.showwarning("Input Error", "Value must be an integer")
            return
            
        for row, col in self.selected_cells:
            self.matrix[row][col] = value
            self.cells[(row, col)].config(text=str(value))
    
    def unselect_all(self):
        for row, col in self.selected_cells.copy():
            self.cells[(row, col)].config(bg="white")
            self.selected_cells.remove((row, col))
    
    def output_matrix(self):
        matrix_name = self.name_var.get() or "mobilityMatrix"
        
        # Calculate maximum width needed for proper alignment
        max_width = 0
        for row in self.matrix:
            for val in row:
                # +1 for negative sign if needed
                width = len(str(val)) + (1 if val < 0 else 0)
                if width > max_width:
                    max_width = width
        
        # Format the matrix as C# code with aligned values
        output = f"int[,] {matrix_name} = new int[8, 8]\n{{\n"
        
        for i, row in enumerate(self.matrix):
            # Format each value with the calculated width
            row_str = ", ".join(f"{x:>{max_width}}" for x in row)
            output += f"    {{ {row_str} }}"
            if i < 7:
                output += ","
            output += "\n"
        
        output += "};"
        
        # Copy to clipboard
        self.root.clipboard_clear()
        self.root.clipboard_append(output)
        
        # Show success message
        messagebox.showinfo("Success", "Matrix copied to clipboard!")
        
        # Print to console as well
        print(output)
    
    def import_from_clipboard(self):
        try:
            # Get content from clipboard
            clipboard_content = self.root.clipboard_get()
            
            # Try to extract matrix name if present
            name_match = re.search(r"int\[\,\]\s*(\w+)\s*=", clipboard_content)
            if name_match:
                self.name_var.set(name_match.group(1))
            
            # Extract matrix values
            matrix_values = []
            for line in clipboard_content.splitlines():
                # Look for lines with matrix values
                if line.strip().startswith('{'):
                    # Remove braces and whitespace, then split by commas
                    values_str = line.strip().replace('{', '').replace('}', '').strip()
                    if values_str:
                        try:
                            row_values = [int(val.strip()) for val in values_str.split(',') if val.strip()]
                            if len(row_values) == 8:
                                matrix_values.append(row_values)
                        except ValueError:
                            continue
            
            # Validate we have exactly 8 rows
            if len(matrix_values) != 8:
                messagebox.showerror("Import Error", "Could not find a valid 8x8 matrix in clipboard")
                return
            
            # Update our matrix and UI
            for i in range(8):
                for j in range(8):
                    self.matrix[i][j] = matrix_values[i][j]
                    self.cells[(i, j)].config(text=str(matrix_values[i][j]))
            
            # Clear selection
            self.unselect_all()
            
            messagebox.showinfo("Success", "Matrix imported from clipboard!")
            
        except tk.TclError:
            messagebox.showerror("Import Error", "No data available in clipboard")
        except Exception as e:
            messagebox.showerror("Import Error", f"Failed to import matrix: {str(e)}")

if __name__ == "__main__":
    root = tk.Tk()
    app = MobilityMatrixCreator(root)
    root.mainloop()